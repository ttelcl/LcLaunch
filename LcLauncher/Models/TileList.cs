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

using Newtonsoft.Json;

namespace LcLauncher.Models;

/// <summary>
/// An ordered list of tiles, together with an ID.
/// This is not serialized direcly, but wraps the
/// result or source of serialization.
/// </summary>
public class TileList
{
  /// <summary>
  /// Create a new TileList. Use <see cref="Load"/> or
  /// <see cref="Create(Guid?)"/> to call this constructor.
  /// </summary>
  private TileList(
    Guid id,
    IEnumerable<TileData?> tiles)
  {
    Id = id;
    Tiles = tiles.ToList();
  }

  /// <summary>
  /// Create a brand new empty TileList, optionally using
  /// a predefined ID.
  /// </summary>
  /// <param name="id">
  /// The optional ID to use. If null, a new ID is generated.
  /// </param>
  /// <returns></returns>
  public static TileList Create(
    Guid? id = null)
  {
    return new TileList(id ?? Guid.NewGuid(), []);
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
  public static TileList? Load(
    LcLaunchDataStore store,
    Guid id)
  {
    var list = store.LoadData<TileData?[]>(
      id, ".tile-list");
    return list == null ? null : new TileList(id, list);
  }

  public void Save(
    LcLaunchDataStore store)
  {
    store.SaveData(Id.ToString(), ".tile-list", Tiles);
  }

  public Guid Id { get; }

  public List<TileData?> Tiles { get; }
}
