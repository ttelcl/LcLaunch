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
    Label =
      Key == null
      ? "<no rack loaded>"
      : $"{Key.StoreName} ({Key.ProviderName})";
  }

  public MainViewModel Host { get; }

  public StoreKey? Key { get; }

  public bool IsValid => Key != null;

  public string RackName => Key?.StoreName ?? "<no rack loaded>";

  public string ProviderName => Key?.ProviderName ?? "-";

  public string Label { get; }

  public RackViewModel? Load()
  {
    if(Key is null)
    {
      return null;
    }
    var hyperStore = Host.HyperStore;
    var rackStore = hyperStore.GetRackStore(Key); // may create the store
    var rackModel = new RackModel3(rackStore, Key); // may create the rack record
    return new RackViewModel(Host, rackModel);
  }
}
