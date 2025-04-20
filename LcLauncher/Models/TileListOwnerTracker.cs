/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LcLauncher.Models;

/// <summary>
/// Tracks 'ownership' for a tile list. This object is shared
/// by all tile list models, and cached in the rack model.
/// </summary>
public class TileListOwnerTracker
{
  private readonly List<ITileListOwner> _claimers;

  public TileListOwnerTracker(
    Guid tileListId)
  {
    _claimers = new List<ITileListOwner>();
    TileListId = tileListId;
  }

  public Guid TileListId { get; }

  /// <summary>
  /// Try to claim ownership, registering the intent to claim
  /// ownership. Returns true if <paramref name="claimer"/> is
  /// now considered the owner of this list.
  /// </summary>
  public bool ClaimOwnerShip(ITileListOwner claimer)
  {
    if(claimer.TargetTilelist.ClaimTracker != this)
    {
      throw new InvalidOperationException(
        $"ITileListOwner.TargetTileList does not match");
    }
    if(!_claimers.Contains(claimer))
    {
      _claimers.Add(claimer);
    }
    return _claimers[0] == claimer;
  }

  /// <summary>
  /// Release the ownership claim to this list. Returns
  /// false if it wasn't a claimer in the first place.
  /// </summary>
  public bool ReleaseOwnerShip(ITileListOwner claimer)
  {
    if(claimer.TargetTilelist.ClaimTracker != this)
    {
      throw new InvalidOperationException(
        $"ITileListOwner.TargetTileList does not match");
    }
    return _claimers.Remove(claimer);
  }

  public ITileListOwner? Owner {
    get => _claimers.Count == 0 ? null : _claimers[0];
  }

}
