/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.DataModel.Entities;

using Ttelcl.Persistence.API;

namespace LcLauncher.DataModel.Store;

/// <summary>
/// Wraps a <see cref="IBucketStore"/> dedicated to storing
/// everything for a single rack
/// </summary>
public class LauncherRackStore
{
  /// <summary>
  /// The one <see cref="RackData"/> instance in this store. May be null
  /// during initialization. Use <see cref="FindRack"/> or <see cref="GetRack"/>
  /// to safely access.
  /// </summary>
  private RackData? _rack;

  /// <summary>
  /// Create a new LauncherRackStore. Note that the one <see cref="RackData"/>
  /// instance is initialized at first call to <see cref="GetRack"/>.
  /// </summary>
  public LauncherRackStore(
    LauncherHyperStore owner,
    StoreKey key)
  {
    Owner = owner;
    Key = key;
    RackStore = Owner.Backing.GetStore(Key);
    if(RackStore is not IJsonBucketStore jsonStore)
    {
      throw new InvalidOperationException(
        "Expecting the rack store to support JSON buckets");
    }
    if(RackStore is not IBlobBucketStore blobStore)
    {
      throw new InvalidOperationException(
        "Expecting the rack store to support BLOB buckets");
    }
    if(RackStore is not ISingletonStore singletonStore)
    {
      throw new InvalidOperationException(
        "Expecting the rack store to support singleton storage");
    }
    ByTypeStore = singletonStore;
    RackBucket = RackStore.GetJsonBucket<RackData>("rack", true)!;
    ShelfBucket = RackStore.GetJsonBucket<ShelfData>("shelf", true)!;
    TileListBucket = RackStore.GetJsonBucket<TileListData>("tiles", true)!;
    IconBucket = RackStore.GetBlobBucket("icon", true)!;
  }

  /// <summary>
  /// The hyper store that this rack is part of
  /// </summary>
  public LauncherHyperStore Owner { get; }

  /// <summary>
  /// The store's key
  /// </summary>
  public StoreKey Key { get; }

  /// <summary>
  /// The name of this rack (as determined by the store's folder name)
  /// </summary>
  public string RackName => Key.StoreName;

  /// <summary>
  /// Retrieve or initialize the one <see cref="RackData"/> instance
  /// in this store. If successful before this will return the
  /// cached version that was returned earlier.
  /// Builds on <see cref="FindRack"/> and creates a new empty rack if
  /// that returned null.
  /// </summary>
  public RackData GetRack()
  {
    var rack = FindRack();
    if(rack != null)
    {
      return rack;
    }
    // Rack not yet initialized: create a new empty one
    _rack = new RackData(
      TickId.New(),
      Key.StoreName,
      [
        new ColumnData(TickId.New(), [], "Column 1"),
        new ColumnData(TickId.New(), [], "Column 2"),
        new ColumnData(TickId.New(), [], "Column 3"),
      ]);
    PutRack(_rack);
    return _rack;
  }

  /// <summary>
  /// Get the one rack instance in this store. Returns the previously
  /// cached version if available. Initializes it from the rack singleton
  /// if possible otherwise. Returns null if still not found.
  /// Use <see cref="GetRack"/> instead to do all this, but auto-initialize
  /// a new instance instead of returning null.
  /// </summary>
  /// <returns></returns>
  public RackData? FindRack()
  {
    if(_rack is not null)
    {
      return _rack;
    }
    if(ByTypeStore.TryGetSingleton("rack", out _rack, RackName))
    {
      return _rack;
    }
    // Rack not yet initialized, neither in this store object, nor in
    // the persistence layer.
    return _rack; // will be null
  }

  /// <summary>
  /// Replace the one rack instance (saving it)
  /// </summary>
  /// <param name="rack"></param>
  public void PutRack(RackData rack)
  {
    ByTypeStore.PutSingleton("rack", rack, RackName);
    _rack = rack;
  }

  /// <summary>
  /// Get a shelf by ID, returning null if not found
  /// </summary>
  /// <param name="shelfId">
  /// The ID of the shelf to find
  /// </param>
  /// <returns></returns>
  public ShelfData? GetShelf(TickId shelfId)
  {
    return ShelfBucket[shelfId];
  }

  /// <summary>
  /// Store the given shelf (at its ID)
  /// </summary>
  public void PutShelf(ShelfData shelf)
  {
    ShelfBucket.PutStorable(shelf);
  }

  /// <summary>
  /// Delete the shelf with the given ID
  /// </summary>
  public void DeleteShelf(TickId shelfId)
  {
    ShelfBucket.Delete(shelfId);
  }

  /// <summary>
  /// Get the specified tiles list (or null if not found)
  /// </summary>
  public TileListData? GetTiles(TickId tilesId)
  {
    return TileListBucket[tilesId];
  }

  /// <summary>
  /// Save the given tiles list
  /// </summary>
  public void PutTiles(TileListData tiles)
  {
    TileListBucket.PutStorable(tiles);
  }

  /// <summary>
  /// Delete the specified tiles list
  /// </summary>
  public void DeleteTiles(TickId tilesId)
  {
    TileListBucket.Delete(tilesId);
  }

  /// <summary>
  /// Open a short lived disposable object for reading icon blobs
  /// </summary>
  public IBlobBucketReader OpenIconReader()
  {
    return IconBucket.OpenReader();
  }

  /// <summary>
  /// Open a short lived disposable object for writing icon blobs
  /// </summary>
  /// <returns></returns>
  public IBlobBucketWriter OpenIconWriter()
  {
    return IconBucket.OpenWriter();
  }

  /// <summary>
  /// Delete everything in the rack
  /// </summary>
  public void Erase()
  {
    RackBucket.Erase();
    _rack = null;
    ShelfBucket.Erase();
    TileListBucket.Erase();
    IconBucket.Erase();
    ByTypeStore.EraseSingletons();
  }

  /// <summary>
  /// The store for items that do not have a natural key. Read: RackData.
  /// </summary>
  public ISingletonStore ByTypeStore { get; }

  /// <summary>
  /// The bucket storing rack top level data (expected to contain 1 item)
  /// </summary>
  [Obsolete("to be replaced")]
  public IJsonBucket<RackData> RackBucket { get; }

  /// <summary>
  /// The bucket containing shelf top level data
  /// </summary>
  public IJsonBucket<ShelfData> ShelfBucket { get; }

  /// <summary>
  /// The bucket containing tile list records
  /// </summary>
  public IJsonBucket<TileListData> TileListBucket { get; }

  /// <summary>
  /// The bucket caching icons used in the tiles of this rack
  /// </summary>
  public IBlobBucket IconBucket { get; }

  /// <summary>
  /// The basic bucket store for rack data.
  /// </summary>
  public IBucketStore RackStore { get; }

}
