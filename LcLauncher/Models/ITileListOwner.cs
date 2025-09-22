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
/// An object that has or wants to claim ownership of a tile list.
/// Implemented by <see cref="ShelfViewModel"/> and 
/// <see cref="GroupTileViewModel"/>
/// </summary>
public interface ITileListOwner
{
  TileListOwnerTracker ClaimTracker { get; }

  /// <summary>
  /// A label for UI purposes. This is used to identify the owner
  /// in error messages and logging.
  /// </summary>
  string TileListOwnerLabel { get; }

  /// <summary>
  /// If true, the ownership claim should take priority over
  /// ones without this flag. This is used to prioritize
  /// shelves over groups.
  /// </summary>
  public bool ClaimPriority { get; }
}

public static class TileListOwnerExtensions
{
  public static bool OwnsTileList(
    this ITileListOwner owner)
  {
    return owner.ClaimTracker.Owner == owner;
  }

  public static bool ClaimTileList(this ITileListOwner owner)
  {
    return owner.ClaimTracker.ClaimOwnerShip(owner);
  }

  public static bool ReleaseTileList(this ITileListOwner owner)
  {
    return owner.ClaimTracker.ReleaseOwnerShip(owner);
  }
}

