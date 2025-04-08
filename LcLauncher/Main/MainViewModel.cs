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


    PageColumns.Add(new PageColumnViewModel(this));
    PageColumns.Add(new PageColumnViewModel(this));
    PageColumns.Add(new PageColumnViewModel(this));
    TestPane = new TestPaneViewModel(this);
  }

  public ILcLaunchStore Store => StoreImplementation;

  public JsonLcLaunchStore StoreImplementation { get; }

  public JsonDataStore FileStore { get => StoreImplementation.Provider; }

  public List<PageColumnViewModel> PageColumns { get; } = [];

  public PageColumnViewModel ColumnA => PageColumns[0];

  public PageColumnViewModel ColumnB => PageColumns[1];

  public PageColumnViewModel ColumnC => PageColumns[2];

  public TestPaneViewModel TestPane { get; }

  public RackViewModel? CurrentRack {
    get => _currentRack;
    set {
      if(SetNullableInstanceProperty(ref _currentRack, value))
      {
      }
    }
  }
  private RackViewModel? _currentRack;
}
