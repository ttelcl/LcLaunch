/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class RackListViewModel: ViewModelBase
{
  public RackListViewModel(
    MainViewModel host)
  {
    Host = host;
    NoRack = new RackReferenceViewModel(host, null);
    _racks = [NoRack];
    _selected = NoRack;
    Refresh();
  }

  public MainViewModel Host { get; }

  public RackReferenceViewModel NoRack { get; }

  public List<RackReferenceViewModel> Racks {
    get => _racks;
    set {
      if(SetInstanceProperty(ref _racks, value))
      {
      }
    }
  }
  private List<RackReferenceViewModel> _racks;

  public RackReferenceViewModel Selected {
    get => _selected;
    set {
      if(SetInstanceProperty(ref _selected, value))
      {
        Trace.TraceInformation(
          $"Rack selection now is: {_selected.Key?.ToString() ?? "<NONE>"}");
      }
    }
  }
  private RackReferenceViewModel _selected;

  public void Refresh()
  {
    var current = _selected;
    
    var rackRefs = new List<RackReferenceViewModel> {};
    var store = Host.HyperStore;
    foreach(var key in store.FindRackStores())
    {
      // try to preserve existing entry
      var vm =
        _racks.FirstOrDefault(rrvm => rrvm.Key == key)
        ?? new RackReferenceViewModel(Host, key);
      rackRefs.Add(vm);
    }
    var newList = new List<RackReferenceViewModel> {
      NoRack
    };
    newList.AddRange(
      from vm in rackRefs
      orderby vm.RackName, vm.ProviderName
      select vm);
    Racks = newList;

    var currentKey = _selected.Key;
    if(currentKey != null)
    {
      if(!_racks.Any(vm => vm.Key == currentKey))
      {
        // the current rack no longer exists
        _selected = NoRack;
      }
    }

    Trace.TraceInformation(
      $"Available racks: {String.Join(", ", _racks.Select(r => r.RackName))}");
  }

  public void OnDropDownOpened()
  {
    Refresh();
  }
}
