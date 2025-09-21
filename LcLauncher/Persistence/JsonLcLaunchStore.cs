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

using LcLauncher.ModelsV2;
using LcLauncher.Storage;

namespace LcLauncher.Persistence;

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
    _iconCacheCache = [];
  }

  public JsonDataStore Provider { get; }

  public IEnumerable<string> EnumRacks()
  {
    return 
      Provider.EnumDataTags(".rack-json")
      .Where(tag => LcLaunchStore.TestValidRackName(tag) == null)
      .ToList();
  }

  public RackData? LoadRack(string rackName)
  {
    LcLaunchStore.ValidateRackName(rackName);
    var rackdata = Provider.LoadData<RackData>(
      rackName,
      ".rack-json");
    return rackdata;
  }

  public void SaveRack(string rackName, RackData rack)
  {
    LcLaunchStore.ValidateRackName(rackName);
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

  /// <inheritdoc/>
  public ILauncherIconCache GetIconCache(Guid cacheId)
  {

    if(_iconCacheCache.TryGetValue(cacheId, out var weakRef))
    {
      if(weakRef.TryGetTarget(out var oldCache))
      {
        return oldCache;
      }
      else
      {
        // Remove expired cache
        _iconCacheCache.Remove(cacheId);
      }
    }
    var cache = CreateIconCache(cacheId, false);
    _iconCacheCache[cacheId] = new WeakReference<ILauncherIconCache>(cache);
    return cache;
  }

  private Dictionary<Guid, WeakReference<ILauncherIconCache>> _iconCacheCache;
    

  private ILauncherIconCache CreateIconCache(Guid cacheId, bool initialize)
  {
    var host = Provider.GetIconCache(cacheId);
    if(initialize)
    {
      host.Initialize();
    }
    return new FileBasedIconCache(host);
  }

}
