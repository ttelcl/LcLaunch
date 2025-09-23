/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.WpfUtilities;

using Ttelcl.Persistence.API;

namespace LcLauncher.Main.Rack;

public class RackListViewModel: ViewModelBase
{
  public RackListViewModel(
    MainViewModel host, string? defaultRackName)
  {
    Host = host;
    NoRack = new RackReferenceViewModel(host, null);
    _racks = [NoRack];
    _selected = NoRack;
    Refresh();
    var defaultRack =
      _racks.FirstOrDefault(r => r.Key?.StoreName == defaultRackName)
      ?? NoRack;
    _selected = defaultRack;
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
      if(!Racks.Contains(value))
      {
        Trace.TraceError(
          "Patching attempt to select unknown rack - to 'no rack'");
        value = NoRack;
      }
      if(SetInstanceProperty(ref _selected, value))
      {
        Trace.TraceInformation(
          $"Rack selection now is: {_selected.Key?.ToString() ?? "<NONE>"}");
        if(_selected.Key is null)
        {
          Host.CurrentRack = null;
        }
        else
        {
          var model = value.Load();
          Host.CurrentRack = model;
        }
      }
    }
  }
  private RackReferenceViewModel _selected;

  public void Refresh()
  {
    var current = _selected;

    var rackRefs = new List<RackReferenceViewModel> { };
    var store = Host.HyperStore;
    foreach(var key in store.FindRackStores())
    {
      // try to preserve existing entry
      var vm =
        _racks.FirstOrDefault(rrvm => rrvm.Key == key)
        ?? new RackReferenceViewModel(Host, key);
      rackRefs.Add(vm);
    }

    var grouped =
      from rack in rackRefs
      group rack by rack?.Key?.StoreName;
    foreach(var group in grouped)
    {
      var items = group.ToList();
      var ambiguous = items.Count > 1;
      foreach(var item in items)
      {
        item.IsAmbiguous = ambiguous;
      }
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

  /// <summary>
  /// Try to select the rack based on the given command line argument.
  /// This current implementation expects a *.rack-json file
  /// as used inside a store or a rack folder names. In case of a file
  /// name, its folder name is checked first (because that can
  /// disambiguate the case of multiple storage providers for the
  /// same rack name)
  /// </summary>
  /// <param name="arg"></param>
  /// <returns></returns>
  public bool SelectFromCommandlineArgument(string arg)
  {
    var segments = arg.Split(
      [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
      StringSplitOptions.RemoveEmptyEntries);
    var keySegments = new List<string>();
    if(segments.Length > 1)
    {
      // put folder name first, since it supports distinguishing providers
      keySegments.Add(segments[^2]);
    }
    if(segments.Length > 0)
    {
      keySegments.Add(segments[^1]);
    }
    foreach(var ks in keySegments)
    {
      // try as rack folder name
      var key = StoreKey.TryParse(ks);
      if(key != null)
      {
        if(SelectFromStoreKey(key))
        {
          return true;
        }
      }
      // try as rack file name
      var parts = ks.Split('.');
      if(parts.Length==3
        && parts[0].Equals("singleton", StringComparison.OrdinalIgnoreCase)
        && parts[2].Equals("rack-json"))
      {
        var candidate = parts[1];
        if(NamingRules.IsValidStoreName(candidate))
        {
          if(SelectFromRackName(candidate))
          {
            return true;
          }
        }
      }
    }
    return false;
  }

  /// <summary>
  /// Select the (first) rack with the given <paramref name="rackName"/>.
  /// </summary>
  /// <param name="rackName"></param>
  /// <returns>
  /// True if the selection was set, false if not found
  /// </returns>
  public bool SelectFromRackName(string rackName)
  {
    var racks =
      _racks
      .Where(
        r => r.RackName.Equals(
          rackName, StringComparison.OrdinalIgnoreCase))
      .ToList();
    if(racks.Count > 0)
    {
      var selection = racks[0];
      if(racks.Count > 1)
      {
        Trace.TraceWarning(
          $"Ambiguous rack reference: '{rackName}'. Picking '{selection}'");
      }
      Selected = selection;
      return true;
    }
    return false;
  }

  /// <summary>
  /// Select the rack with the given <paramref name="key"/>.
  /// </summary>
  /// <param name="key"></param>
  /// <returns>
  /// True if the selection was set, false if not found
  /// </returns>
  public bool SelectFromStoreKey(StoreKey? key)
  {
    var rack =
      _racks
      .FirstOrDefault(r => r.Key == key);
    if(rack != null)
    {
      Selected = rack;
      return true;
    }
    return false;
  }

  public void OnDropDownOpened()
  {
    Refresh();
  }
}
