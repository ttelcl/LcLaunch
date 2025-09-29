/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

using Ttelcl.Persistence.API;

namespace LcLauncher.Main.Rack;

public class RackReferenceViewModel: ViewModelBase
{
  public RackReferenceViewModel(
    RackListViewModel rackList,
    StoreKey? key)
  {
    RackList = rackList;
    Key = key;
    DullColor = Brushes.Gray;
    AlertColor = Brushes.DarkOrange;
    SelectThisCommand = new DelegateCommand(
      p => { RackList.Selected = this; },
      p => Key != null);
  }

  public ICommand SelectThisCommand { get; }

  public RackListViewModel RackList { get; }
  
  public MainViewModel Host => RackList.Host;

  public StoreKey? Key { get; }

  public bool IsValid => Key != null;

  public bool IsAmbiguous {
    get => _isAmbiguous;
    set {
      if(SetValueProperty(ref _isAmbiguous, value))
      {
        RaisePropertyChanged(nameof(Label));
        RaisePropertyChanged(nameof(ProviderColor));
      }
    }
  }
  private bool _isAmbiguous = false;

  private const string __noRackName = "--- rack management ---";

  public string RackName => Key?.StoreName ?? __noRackName;

  public string ProviderName => Key?.ProviderName ?? "-";

  public string Label {
    get {
      if(Key == null)
      {
        return __noRackName;
      }
      return
        IsAmbiguous
        ? $"{Key.StoreName} ({Key.ProviderName})"
        : Key.StoreName;
    }
  }

  public Brush ProviderColor =>
    IsAmbiguous ? AlertColor : DullColor;

  public Brush DullColor { get; }

  public Brush AlertColor { get; }

  public RackViewModel? Load()
  {
    if(Key is null)
    {
      return null;
    }
    var hyperStore = Host.HyperStore;
    var rackStore = hyperStore.GetRackStore(Key); // may create the store
    var rackModel = new RackModel(rackStore, Key); // may create the rack record
    return new RackViewModel(Host, rackModel);
  }
}
