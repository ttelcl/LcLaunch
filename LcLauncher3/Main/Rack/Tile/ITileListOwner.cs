/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// An object owning a tile list (a shelf or a group tile)
/// </summary>
public interface ITileListOwner
{
  /// <summary>
  /// The list of tiles owned by this <see cref="ITileListOwner"/>
  /// </summary>
  TileListViewModel OwnedTiles { get; }

  /// <summary>
  /// The tile list containing this <see cref="ITileListOwner"/>.
  /// The hosting tile list for group tiles, or null for shelves
  /// and rogue group tiles.
  /// </summary>
  TileListViewModel? ParentTiles { get; }

  /// <summary>
  /// The rack that this owner (and its tile list) is in
  /// </summary>
  RackViewModel Rack { get; }

  /// <summary>
  /// Callback after <see cref="OwnedTiles"/> has been edited
  /// </summary>
  void OwnedTilesEdited();
}

public static class TileListOwner
{
  /// <summary>
  /// Walk the chain of <see cref="ITileListOwner"/>s to find
  /// the root <see cref="ShelfViewModel"/>.
  /// </summary>
  /// <param name="tileListOwner">
  /// The <see cref="ITileListOwner"/> to start at
  /// </param>
  /// <param name="maxDepth">
  /// Default 10. Maximum supported nesting level of group tiles.
  /// </param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static ShelfViewModel GetShelf(
    this ITileListOwner tileListOwner,
    int maxDepth = 10)
  {
    if(tileListOwner is ShelfViewModel svm)
    {
      return svm;
    }
    var parentOwner = tileListOwner.ParentTiles?.Owner;
    if(parentOwner == null)
    {
      throw new InvalidOperationException(
        "Cannot locate shelf for ITileListOwner: No parent tile list available" +
        $" for owner of tile list {tileListOwner.OwnedTiles.TileListId}");
    }
    if(maxDepth <= 0)
    {
      throw new InvalidOperationException(
        "Cannot locate shelf for ITileListOwner: Recusrion limit exceeded" +
        $" at tile list {parentOwner.OwnedTiles.TileListId}");
    }
    return parentOwner.GetShelf(maxDepth-1);
  }
}

