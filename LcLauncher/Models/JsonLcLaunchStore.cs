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

using LcLauncher.Storage;

namespace LcLauncher.Models;

/// <summary>
/// Implements <see cref="ILcLaunchStore"/> using JSON files.
/// </summary>
public class JsonLcLaunchStore: ILcLaunchStore
{
  /// <summary>
  /// Create a new JsonLcLaunchStore
  /// </summary>
  public JsonLcLaunchStore(
    JsonDataStore provider)
  {
    Provider = provider;
  }

  public JsonDataStore Provider { get; }

  public IEnumerable<string> EnumRacks()
  {
    return Provider.EnumDataTags(".rack-json").ToList();
  }

  public RackData LoadRack(string rackName)
  {
    var rackdata = Provider.LoadData<RackData>(
      rackName,
      ".rack-json");
    return rackdata ?? throw new FileNotFoundException(
      "Rack file not found",
      rackName);
  }

  public void SaveRack(string rackName, RackData rack)
  {
    Provider.SaveData(
      rackName,
      ".rack-json",
      rack);
  }

  public ShelfData? LoadShelf(Guid shelfId)
  {
    var shelfData = Provider.LoadData<ShelfData>(
      shelfId,
      ".shelf-json");
    return shelfData;
  }

  public void SaveShelf(Guid id, ShelfData shelf)
  {
    Provider.SaveData(
      id,
      ".shelf-json",
      shelf);
  }

  public List<TileData?>? LoadTiles(Guid tileId)
  {
    var listData = Provider.LoadData<List<TileData?>>(
      tileId,
      ".tiles-json");
    return listData;
  }

  public void SaveTiles(Guid id, IEnumerable<TileData?> tiles)
  {
    Provider.SaveData(
      id,
      ".tiles-json",
      tiles.ToList());
  }

  public IEnumerable<Guid> EnumShelves()
  {
    foreach(var tag in Provider.EnumDataTags(".shelf-json"))
    {
      if(Guid.TryParse(tag, out var id))
      {
        yield return id;
      }
    }
  }

  public IEnumerable<Guid> EnumTiles()
  {
    foreach(var tag in Provider.EnumDataTags(".tiles-json"))
    {
      if(Guid.TryParse(tag, out var id))
      {
        yield return id;
      }
    }
  }
}
