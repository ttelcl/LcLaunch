/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.Models;

/// <summary>
/// Abstract store of LcLaunch persistent data.
/// </summary>
public interface ILcLaunchStore
{
  /// <summary>
  /// Enumerate all existing rack names in the store.
  /// </summary>
  IEnumerable<string> EnumRacks();

  /// <summary>
  /// Load a rack by name.
  /// </summary>
  RackData LoadRack(string rackName);

  /// <summary>
  /// Save a rack to the store.
  /// </summary>
  void SaveRack(string rackName, RackData rack);

  /// <summary>
  /// Load a shelf by ID.
  /// </summary>
  ShelfData? LoadShelf(Guid shelfId);

  /// <summary>
  /// Save a shelf to the store.
  /// </summary>
  void SaveShelf(Guid id, ShelfData shelf);

  /// <summary>
  /// Load a tile list by ID.
  /// </summary>
  List<TileData0?>? LoadTiles(Guid tileId);

  /// <summary>
  /// Save a tile list to the store.
  /// </summary>
  void SaveTiles(Guid id, IEnumerable<TileData0?> tiles);

  /// <summary>
  /// Enumerate all existing shelves in the store.
  /// </summary>
  IEnumerable<Guid> EnumShelves();

  /// <summary>
  /// Enumerate all existing tile lists in the store.
  /// </summary>
  IEnumerable<Guid> EnumTiles();
}

