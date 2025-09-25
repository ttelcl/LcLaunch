using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using Microsoft.Extensions.Configuration;

using Ttelcl.Persistence.API;

using LcLauncher.DataModel.Store;
using LcLauncher.ShellApps;
using LcLauncher.WpfUtilities;
using LcLauncher.Main.Rack;

namespace LcLauncher.Main;

public class MainViewModel: ViewModelBase
{
  private readonly DispatcherTimer _iconJobTimer;

  public MainViewModel(IConfigurationRoot configuration)
  {
    AppCache = new ShellAppCache(false);
    Configuration = configuration;
    DefaultTheme = configuration["defaultTheme"] ?? DefaultDefaultTheme;
    ShowDevPane = configuration.GetValue<bool>("showDevPane", false);
    HyperStore = InitHyperStore();
    DefaultStore = HyperStore.Backing.GetStore("default");

    // Make sure there is at least one rack (named "default")
    Trace.TraceError("TODO: create default rack if missing");

    //Store.LoadOrCreateRack("default");
    RackList = new RackListViewModel(this, configuration["defaultRack"]);
    ProcessNextIconJobCommand = new DelegateCommand(
      p => ProcessNextIconJob(),
      p => CanProcessNextIconJob());
    _iconJobTimer = new DispatcherTimer(
      DispatcherPriority.ApplicationIdle) {
      Interval = TimeSpan.FromMilliseconds(30 /*1500*/),
      IsEnabled = false,
    };
    _iconJobTimer.Tick += (s, e) => {
      if(!ProcessNextIconJob())
      {
        _iconJobTimer.Stop();
        Trace.TraceInformation(
          $"Rack Queue is now stopped");
      }
    };
    DevReloadAppsCommand = new DelegateCommand(
      p => { AppCache.Refill(TimeSpan.FromMinutes(1)); }
    );
    DevDumpAppsCommand = new DelegateCommand(
      p => { DevDumpApps(); });
    DevTogglePaneCommand = new DelegateCommand(
      p => { ShowDevPane = !ShowDevPane; });
  }

  public IConfigurationRoot Configuration { get; }

  public ICommand DevReloadAppsCommand { get; }

  public ICommand DevDumpAppsCommand { get; }

  public ICommand DevTogglePaneCommand { get; }

  public ICommand ProcessNextIconJobCommand { get; }

  public ShellAppCache AppCache { get; }

  public LauncherHyperStore HyperStore { get; }

  /// <summary>
  /// A bucket store for miscellaneous stuff, e.g. testing dumps
  /// </summary>
  public IBucketStore DefaultStore { get; }

  public RackViewModel? CurrentRack {
    get => _currentRack;
    set {
      var oldRack = _currentRack;
      if(SetNullableInstanceProperty(ref _currentRack, value))
      {
        var rackLabel = value?.Name ?? "<NONE>";
        Trace.TraceInformation(
          $"Switched to rack '{rackLabel}'");
        if(oldRack != null)
        {
          oldRack.Unload();
        }
        ActivateRackIconQueue();
      }
    }
  }
  private RackViewModel? _currentRack;

  public string DefaultTheme { get; }

  public const string DefaultDefaultTheme = "Olive";

  public bool ShowDevPane {
    get => _showDevPane;
    set {
      if(SetValueProperty(ref _showDevPane, value))
      {
      }
    }
  }
  private bool _showDevPane;

  public RackListViewModel RackList { get; }

  public void ActivateRackIconQueue()
  {
    if(
      !_iconJobTimer.IsEnabled
      && CurrentRack != null
      && CurrentRack.IconQueue.HasWork)
    {
      Trace.TraceInformation(
        $"Rack Queue is now active");
      _iconJobTimer.IsEnabled = true;
    }
  }

  public bool ProcessNextIconJob()
  {
    if(CurrentRack == null || !CurrentRack.IconQueue.HasWork)
    {
      return false;
    }
    return CurrentRack.IconLoader.ProcessNextBatch(
      CurrentRack.IconQueue, 10);
  }

  public bool CanProcessNextIconJob()
  {
    return CurrentRack != null && CurrentRack.IconQueue.HasWork;
  }

  private void DevDumpApps()
  {
    AppCache.Refill(TimeSpan.FromMinutes(5));
    var apps = AppCache.Descriptors.ToList();
    var appsSorted =
      from app in apps
      orderby app.Kind, app.Label
      select app;
    var grouped =
      appsSorted
      .GroupBy(descriptor => descriptor.Kind)
      .ToDictionary(g => g.Key.ToString(), g => g.ToList());
    DefaultStore
      .GetJsonBucket<Dictionary<string,List<ShellAppDescriptor>>>("app-dump", true)!
      .Put(TickId.New(), grouped);
  }

  public void OnWindowClosing()
  {
    if(CurrentRack != null)
    {
      CurrentRack = null;
    }
  }

  public void OnAppActiveChange(bool active)
  {
    if(active)
    {
      Trace.TraceInformation(
        $"Application is now active");
    }
    else
    {
      Trace.TraceInformation(
        $"Application is now inactive");
      if(CurrentRack != null)
      {
        Trace.TraceError("NYI: saving rack upon app move to background");
        //CurrentRack.SaveShelvesIfModified();
        //CurrentRack.SaveDirtyTileLists();
        //CurrentRack.SaveIfDirty();
      }
    }
  }

  // Usually set indirectly by the editor's IsActive property
  public EditorViewModelBase? CurrentEditor {
    get => _currentEditor;
    internal set {
      var oldEditor = _currentEditor;
      if(SetNullableInstanceProperty(ref _currentEditor, value))
      {
        if(oldEditor != null)
        {
          oldEditor.IsActive = false;
          Trace.TraceInformation(
            $"Closed editor '{oldEditor.EditorTitle}' ({oldEditor.GetType().Name})");
        }
        if(_currentEditor != null)
        {
          _currentEditor.IsActive = true;
          Trace.TraceInformation(
            $"Switched to editor '{_currentEditor.EditorTitle}' ({_currentEditor.GetType().Name})");
        }
        else
        {
          Trace.TraceInformation(
            $"No editor active");
        }
      }
    }
  }
  private EditorViewModelBase? _currentEditor;

  private static LauncherHyperStore InitHyperStore()
  {
    var fsProvider = new Ttelcl.Persistence.Filesystem.FsBucketProvider();
    var folder = LauncherHyperStore.DefaultStoreFolder;
    var hyperBucketStore =
      new HyperBucketStore(
        folder, [fsProvider]);
    return new LauncherHyperStore(hyperBucketStore);
  }
}
