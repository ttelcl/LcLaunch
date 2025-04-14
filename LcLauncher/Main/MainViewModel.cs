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

using LcLauncher.Main.Rack;
using LcLauncher.Models;
using LcLauncher.Storage;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main;

public class MainViewModel: ViewModelBase
{
  public MainViewModel()
  {
    var fileStore = new JsonDataStore();
    var storeImplementation = new JsonLcLaunchStore(fileStore);
    StoreImplementation = storeImplementation;
    // Make sure there is at least one rack (named "default")
    Store.LoadOrCreateRack("default");
    RackList = new RackListViewModel(this);
    TestPane = new TestPaneViewModel(this);
  }

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

}
