/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LcLauncher.Main.Rack;
using LcLauncher.Main.Rack.Tile;

namespace LcLauncher.Models;

/// <summary>
/// An object has or wants to claim ownership of a tile list.
/// Implemented by <see cref="ShelfViewModel"/> and 
/// <see cref="GroupTileViewModel"/>
/// </summary>
public interface ITileListOwner
{
  /// <summary>
  /// The target tile list. Or more precisely: any TileListModel
  /// pointing to the underlying tile list in the rack.
  /// </summary>
  TileListModel TargetTilelist { get; }

  string TileListOwnerLabel { get; }
}

public static class TileListOwnerExtensions
{
  public static bool OwnsTileList(
    this ITileListOwner owner)
  {
    return owner.TargetTilelist.Owner == owner;
  }

  public static bool ClaimTileList(this ITileListOwner owner)
  {
    return owner.TargetTilelist.ClaimOwnerShip(owner);
  }

  public static bool ReleaseTileList(this ITileListOwner owner)
  {
    return owner.TargetTilelist.ReleaseOwnerShip(owner);
  }
}

