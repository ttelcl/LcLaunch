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
using LcLauncher.Models;
using LcLauncher.Storage;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main;

public class MainViewModel: ViewModelBase
{
  private readonly DispatcherTimer _iconJobTimer;

  public MainViewModel()
  {
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

  public ICommand ProcessNextIconJobCommand { get; }

  public ILcLaunchStore Store => StoreImplementation;

  public JsonLcLaunchStore StoreImplementation { get; }

  public JsonDataStore FileStore { get => StoreImplementation.Provider; }

  public TestPaneViewModel TestPane { get; }

  public RackViewModel? CurrentRack {
    get => _currentRack;
    set {
      if(SetNullableInstanceProperty(ref _currentRack, value))
      {
        var rackLabel = value?.Name ?? "<NONE>";
        Trace.TraceInformation(
          $"Switched to rack '{rackLabel}'");
      }
    }
  }
  private RackViewModel? _currentRack;

  public RackListViewModel RackList { get; }

  public void RackQueueActivating(IconLoadQueue queue)
  {
    Trace.TraceInformation(
      $"Rack Queue is now active");
    _iconJobTimer.IsEnabled = true;
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
}
