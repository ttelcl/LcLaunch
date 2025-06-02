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

using LcLauncher.IconUpdates;
using LcLauncher.Main.Rack;
using LcLauncher.Persistence;
using LcLauncher.Storage;
using LcLauncher.WpfUtilities;

using Microsoft.Extensions.Configuration;

namespace LcLauncher.Main;

public class MainViewModel: ViewModelBase
{
  private readonly DispatcherTimer _iconJobTimer;

  public MainViewModel(IConfigurationRoot configuration)
  {
    Configuration = configuration;
    DefaultTheme = configuration["defaultTheme"] ?? DefaultDefaultTheme;
    ShowDevPane = configuration.GetValue<bool>("showDevPane", false);
    var fileStore = new JsonDataStore();
    var storeImplementation = new JsonLcLaunchStore(fileStore);
    StoreImplementation = storeImplementation;
    // Make sure there is at least one rack (named "default")
    Store.LoadOrCreateRack("default");
    RackList = new RackListViewModel(this);
    TestPane = new TestPaneViewModel(this);
    ProcessNextIconJobCommand = new DelegateCommand(
      p => ProcessNextIconJob(),
      p => CanProcessNextIconJob());
    _iconJobTimer = new DispatcherTimer(
      DispatcherPriority.ApplicationIdle) {
      Interval = TimeSpan.FromMilliseconds(30),
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
  }

  public IConfigurationRoot Configuration { get; }

  public ICommand ProcessNextIconJobCommand { get; }

  public ILcLaunchStore Store => StoreImplementation;

  public JsonLcLaunchStore StoreImplementation { get; }

  public JsonDataStore FileStore { get => StoreImplementation.Provider; }

  public TestPaneViewModel TestPane { get; }

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
          oldRack.SaveShelvesIfModified();
          oldRack.SaveDirtyTileLists();
          oldRack.SaveIfDirty();
        }
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

  public void RackQueueActivating(IconLoadQueue queue)
  {
    if(!_iconJobTimer.IsEnabled)
    {
      Trace.TraceInformation(
        $"Rack Queue is now active");
      _iconJobTimer.IsEnabled = true;
    }
  }

  public bool ProcessNextIconJob()
  {
    var processed = CurrentRack?.IconLoadQueue.ProcessNextJob() ?? false;
    return processed;
  }

  public bool CanProcessNextIconJob()
  {
    return CurrentRack != null &&
      !CurrentRack.IconLoadQueue.IsEmpty();
  }

  public void OnWindowClosing()
  {
    CurrentRack?.SaveShelvesIfModified();
    CurrentRack?.SaveDirtyTileLists();
    CurrentRack?.SaveIfDirty();
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
      CurrentRack?.SaveShelvesIfModified();
      CurrentRack?.SaveDirtyTileLists();
      CurrentRack?.SaveIfDirty();
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
}
