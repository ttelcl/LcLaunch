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
    RackJsonStore = jsonStore;
    if(RackStore is not IBlobBucketStore blobStore)
    {
      throw new InvalidOperationException(
        "Expecting the rack store to support BLOB buckets");
    }
    RackBlobStore = blobStore;
    RackBucket = RackJsonStore.GetJsonBucket<RackData>("rack", true)!;
    ShelfBucket = RackJsonStore.GetJsonBucket<ShelfData>("shelf", true)!;
    TileListBucket = RackJsonStore.GetJsonBucket<TileListData>("tiles", true)!;
    IconBucket = RackBlobStore.GetBlobBucket("icon", true)!;
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
    RackBucket.PutStorable(_rack);
    return _rack;
  }

  /// <summary>
  /// Get the one rack instance in this store. Returns the previously
  /// cached version if available. Initializes it from the rack bucket
  /// if possible otherwise. Returns null if still not found.
  /// Use <see cref="GetRack"/> instead to do all this, but auto-initialize
  /// a new instance instead of returning null.
  /// </summary>
  /// <returns></returns>
  public RackData? FindRack()
  {
    if(_rack is not null)
    {
      if(!RackBucket.ContainsKey(_rack.Id))
      {
        // Somehow the rack object was lost. Erase the cached Rack.
        _rack = null;
      }
      else
      {
        // cache is still valid
        return _rack;
      }
    }
    // _rack is now null (either because it was null at the start or because
    // we cleared it.
    var rackIds = RackBucket.Keys().ToList();
    if(rackIds.Count > 1)
    {
      // we have a problem.
      throw new InvalidOperationException(
        $"Found {rackIds.Count} distinct top level rack records in {Key} (expecting 1)");
    }
    if(rackIds.Count == 1)
    {
      var id = rackIds[0];
      if(!RackBucket.TryFind(id, out _rack))
      {
        throw new InvalidOperationException(
          $"Rack data corrupted. Rack {id} both exists and does not exist in {Key}");
      }
      return _rack;
    }
    // Rack not yet initialized, neither in this store object, nor in
    // the persistence layer.
    return null;
  }

  /// <summary>
  /// Replace the one rack instance (saving it). The ID should match the existing
  /// rack.
  /// </summary>
  /// <param name="rack"></param>
  public void PutRack(RackData rack)
  {
    if(_rack != null && rack.Id != _rack.Id)
    {
      Trace.TraceWarning("Replacing rack with rack with different ID");
      RackBucket[_rack.Id] = null; // delete!
      _rack = null;
    }
    RackBucket.PutStorable(rack);
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
  }

  /// <summary>
  /// The bucket storing rack top level data (expected to contain 1 item)
  /// </summary>
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
  /// The basic bucket store for rack data. <see cref="RackJsonStore"/> 
  /// and <see cref="RackBlobStore"/> are stronger type subinterfaces of this.
  /// </summary>
  public IBucketStore RackStore { get; }

  /// <summary>
  /// The JSON data oriented subinterface of <see cref="RackStore"/>
  /// </summary>
  public IJsonBucketStore RackJsonStore { get; }

  /// <summary>
  /// The Blob data oriented subinterface of <see cref="RackStore"/>
  /// </summary>
  public IBlobBucketStore RackBlobStore { get; }

}
