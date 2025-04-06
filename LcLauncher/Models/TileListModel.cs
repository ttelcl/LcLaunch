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

using LcLauncher.Storage;
using LcLauncher.Storage.BlobsStorage;

using Newtonsoft.Json;

namespace LcLauncher.Models;

/// <summary>
/// An ordered list of tiles, together with an ID.
/// This is not serialized direcly, but wraps the
/// result or source of serialization.
/// </summary>
public class TileList 
{
  private readonly List<TileData0?> _tiles;

  public TileList(
    Guid id,
    IEnumerable<TileData0?> tiles)
  {
    _tiles = tiles.ToList();
    Id = id;
    Tiles = _tiles.AsReadOnly();
  }

  public Guid Id { get; }

  public IReadOnlyList<TileData0?> Tiles { get; }
}

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
    IEnumerable<TileData0?> tiles,
    ILcLaunchStore store)
  {
    Id = id;
    Tiles = tiles.ToList();
    Store = store;
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
    ILcLaunchStore store,
    Guid? id = null)
  {
    return new TileListModel(id ?? Guid.NewGuid(), [], store);
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
    ILcLaunchStore store,
    Guid id)
  {
    var list = store.LoadTiles(id);
    return list == null ? null : new TileListModel(id, list, store);
  }

  public void Save()
  {
    Store.SaveTiles(Id, Tiles);
  }

  public Guid Id { get; }

  public List<TileData0?> Tiles { get; }

  /// <summary>
  /// The store in which this list is saved and its icons are cached.
  /// </summary>
  public ILcLaunchStore Store { get; }

}
