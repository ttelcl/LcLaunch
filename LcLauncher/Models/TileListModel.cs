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
using System.Windows.Media.Imaging;

using LcLauncher.Persistence;
using LcLauncher.Storage;
using LcLauncher.Storage.BlobsStorage;

using Newtonsoft.Json;

namespace LcLauncher.Models;

/// <summary>
/// An ordered list of tiles, together with an ID.
/// This is not serialized direcly, but wraps the
/// result or source of serialization.
/// </summary>
public class TileListModel
{

  /// <summary>
  /// Create a new TileList. Use <see cref="Load"/> or
  /// <see cref="Create(Guid?)"/> to call this constructor.
  /// </summary>
  private TileListModel(
    Guid id,
    IEnumerable<TileData?> tiles,
    RackModel rack)
  {
    Id = id;
    RawTiles = tiles.ToList();
    Store = rack.Store;
    IconCache = Store.GetIconCache(id);
    Rack = rack;
    ClaimTracker = Rack.GetClaimTracker(id);
  }

  /// <summary>
  /// Create a brand new empty TileList, optionally using
  /// a predefined ID.
  /// </summary>
  /// <param name="id">
  /// The optional ID to use. If null, a new ID is generated.
  /// </param>
  /// <returns></returns>
  public static TileListModel Create(
    RackModel rack,
    Guid? id = null)
  {
    return new TileListModel(
      id ?? Guid.NewGuid(),
      [
        TileData.EmptyTile(),
        TileData.EmptyTile(),
        TileData.EmptyTile(),
        TileData.EmptyTile(),
      ],
      rack);
  }

  /// <summary>
  /// Load a TileList from a *.tile-list file in the given folder.
  /// </summary>
  /// <param name="store">
  /// The store in which to look for the file.
  /// </param>
  /// <param name="id">
  /// The ID, implying the file name.
  /// </param>
  /// <returns>
  /// Returns the list that was loaded, or null if the file
  /// did not exist.
  /// </returns>
  public static TileListModel? Load(
    RackModel rack,
    Guid id)
  {
    var list = rack.Store.LoadTiles(id);
    return list == null ? null : new TileListModel(id, list, rack);
  }

  public TileListModel CreateClone()
  {
    SaveRawModel();
    var newId = Guid.NewGuid();
    var tiles = new List<TileData?>(RawTiles); // clone to independent list
    var clone = new TileListModel(
      newId,
      tiles,
      Rack);
    Trace.TraceInformation(
      $"Creating clone of tile list {Id} as {newId}");
    clone.SaveRawModel();
    return clone;
  }

  /// <summary>
  /// Save the existing model. Warning: this is quite possibly
  /// NOT what you want to save, as it may not have all changes.
  /// Use the methods in the ViewModel to rebuild the model from the
  /// viewmodels first.
  /// </summary>
  public void SaveRawModel()
  {
    Store.SaveTiles(Id, RawTiles);
    IsDirty = false;
  }

  public void DevSaveCopy(Guid id)
  {
    Trace.TraceInformation(
      $"Saving copy of tile list {Id} as {id}");
    Store.SaveTiles(id, RawTiles);
  }

  public bool IsDirty { get; private set; }

  public void MarkDirty()
  {
    IsDirty = true;
  }

  public Guid Id { get; }

  public List<TileData?> RawTiles { get; }

  /// <summary>
  /// The rack this list belongs to. This provides the context
  /// in which this list should be unique.
  /// </summary>
  public RackModel Rack { get; }

  /// <summary>
  /// The store in which this list is saved and its icons are cached.
  /// </summary>
  public ILcLaunchStore Store { get; }

  public ILauncherIconCache IconCache { get; }

  public TileListOwnerTracker ClaimTracker { get; }

  /// <summary>
  /// Try to claim ownership, registering the intent to claim
  /// ownership. Returns true if <paramref name="claimer"/> is
  /// now considered the owner of this list.
  /// </summary>
  public bool ClaimOwnerShip(ITileListOwner claimer)
  {
    return ClaimTracker.ClaimOwnerShip(claimer);
  }

  /// <summary>
  /// Release the ownership claim to this list. Returns
  /// false if it wasn't a claimer in the first place.
  /// </summary>
  public bool ReleaseOwnerShip(ITileListOwner claimer)
  {
    return ClaimTracker.ReleaseOwnerShip(claimer);
  }

  public ITileListOwner? Owner {
    get => ClaimTracker.Owner;
  }
}
