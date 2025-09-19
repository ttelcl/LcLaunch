/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Main;

using Ttelcl.Persistence.API;

namespace LcLauncher.ModelConversion;

/// <summary>
/// Temporary model format conversion code
/// </summary>
public class ModelConverter
{
  public ModelConverter(
    MainViewModel host)
  {
    Host = host;
  }

  public MainViewModel Host { get; }

  public void ConvertCurrentRack()
  {
    if(Host.CurrentRack == null)
    {
      Trace.TraceError("There is no current rack!");
      return;
    }
    var sourceRack = Host.CurrentRack.Model.RackData;
    var sourceRackName = Host.CurrentRack.Model.RackName;
    var provider = Host.HyperStore.HyperStore.DefaultProvider;
    var targetKey =
      new StoreKey(provider.ProviderName, "rack", sourceRackName);
    var targetRackStore = Host.HyperStore.GetRackStore(targetKey);
    Trace.TraceInformation($"Erasing store {targetKey}");
    targetRackStore.Erase();
  }

}
