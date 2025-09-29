/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

using Ttelcl.Persistence.API;

namespace LcLauncher.Main.Rack;

public class RackReferenceViewModel: ViewModelBase
{
  public RackReferenceViewModel(
    MainViewModel host,
    StoreKey? key)
  {
    Host = host;
    Key = key;
  }

  public MainViewModel Host { get; }

  public StoreKey? Key { get; }

  public bool IsValid => Key != null;

  public bool IsAmbiguous {
    get => _isAmbiguous;
    set {
      if(SetValueProperty(ref _isAmbiguous, value))
      {
        RaisePropertyChanged(nameof(Label));
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
