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

using Newtonsoft.Json;

using LcLauncher.Storage;
using LcLauncher.Persistence;

namespace LcLauncher.Models;

/// <summary>
/// Manages one Rack database (all related files).
/// </summary>
public class RackModel
{
  private Dictionary<Guid, ShelfModel> _shelves;

  /// <summary>
  /// Create a new RackModel. If the name is unknown, it will be created.
  /// </summary>
  public RackModel(
    ILcLaunchStore store,
    string rackName)
  {
    LcLaunchStore.ValidateRackName(rackName);
    Store = store;
    RackName = rackName;
    _shelves = [];
    Columns = new List<ShelfModel>[3];
    var rackData = Store.LoadOrCreateRack(RackName);
    RackData = rackData;
    // populate shelves in the lookup and the columns
    for(var icol=0; icol<3; icol++)
    {
      Columns[icol] = [];
      var column = rackData.Columns[icol];
      foreach(var shelfId in column)
      {
        if(!_shelves.TryGetValue(shelfId, out var shelf))
        {
          shelf = ShelfModel.Load(this, shelfId);
          _shelves.Add(shelfId, shelf);
        }
        Columns[icol].Add(shelf);
      }
    }
  }

  public ILcLaunchStore Store { get; }

  /// <summary>
  /// The name of the rack (file name without path nor extension).
  /// </summary>
  public string RackName { get; }

  public RackData RackData { get; }

  public IReadOnlyDictionary<Guid, ShelfModel> ShelfMap => _shelves;

  /// <summary>
  /// The 3 columns of the rack.
  /// </summary>
  public List<ShelfModel>[] Columns { get; }

  /// <summary>
  /// Save this rack itself (the column setup).
  /// Does NOT save the shelves
  /// </summary>
  public void Save()
  {
    Store.SaveRack(RackName, RackData);
    IsDirty = false;
  }

  public bool IsDirty { get; private set; }

  public void MarkDirty()
  {
    IsDirty = true;
  }

}
