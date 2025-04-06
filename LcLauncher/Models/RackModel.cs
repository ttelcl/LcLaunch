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

namespace LcLauncher.Models;

/// <summary>
/// Manages one Rack database (all related files).
/// </summary>
public class RackModel
{
  private Dictionary<Guid, ShelfModel> _shelves;

  /// <summary>
  /// Create a new RackDb
  /// </summary>
  public RackModel(
    LcLaunchDataStore store,
    string rackName)
  {
    Store = store;
    RackName = rackName;
    _shelves = [];
    var fullPath = Path.Combine(
      Store.DataFolder,
      RackName + ".rack-json");
    if(!File.Exists(fullPath))
    {
      throw new FileNotFoundException(
        "Rack file not found",
        fullPath);
    }
    var rackData = Store.LoadData<RackData>(
      RackName,
      ".rack-json") ?? throw new InvalidDataException(
        "Failed to load rack data file");
    RackData = rackData;
    // populate shelves in the lookup
    foreach(var column in rackData.Columns)
      foreach(var shelfId in column)
      {
        if(!_shelves.ContainsKey(shelfId))
        {
          var shelf = ShelfModel.Load(this, shelfId);
          _shelves.Add(shelfId, shelf);
        }
      }
    // TODO: create columns and tile lists
  }

  public LcLaunchDataStore Store { get; }

  /// <summary>
  /// The name of the rack (file name without path nor extension).
  /// </summary>
  public string RackName { get; }

  public RackData RackData { get; }

  public IReadOnlyDictionary<Guid, ShelfModel> ShelfMap => _shelves;

}
