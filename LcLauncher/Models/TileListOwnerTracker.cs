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

  private TileListOwnerTracker(
    RackModel host,
    Guid tileListId)
  {
    _claimers = [];
    Host = host;
    TileListId = tileListId;
    Trace.TraceInformation(
      $"TileListOwnerTracker: Created tracker {TrackerId} for list {TileListId}");
  }

  public static TileListOwnerTracker FromRack(
    RackModel host, Guid tileListId)
  {
    if(host.HasClaimTracker(tileListId))
    {
      throw new InvalidOperationException(
        $"TileListOwnerTracker: Claim tracker for {tileListId} already exists");
    }
    return new TileListOwnerTracker(host, tileListId);
  }

  public RackModel Host { get; }

  public Guid TileListId { get; }

  public Guid TrackerId { get; } = Guid.NewGuid();

  /// <summary>
  /// Try to claim ownership, registering the intent to claim
  /// ownership. Returns true if <paramref name="claimer"/> is
  /// now considered the owner of this list.
  /// </summary>
  public bool ClaimOwnerShip(ITileListOwner claimer)
  {
    if(claimer.ClaimTracker != this)
    {
      throw new InvalidOperationException(
        $"ITileListOwner.ClaimedTileListId does not match");
    }
    if(!_claimers.Contains(claimer))
    {
      if(claimer.ClaimPriority && Owner!=null && !Owner.ClaimPriority)
      {
        // override the current owner
        Trace.TraceWarning(
          $"TileListOwnerTracker: {claimer.TileListOwnerLabel} "+
          $"overrides {Owner?.TileListOwnerLabel ?? String.Empty} for "+
          $"list {TileListId}");
        _claimers.Insert(0, claimer);
      }
      else
      {
        _claimers.Add(claimer);
      }
    }
    return _claimers[0] == claimer;
  }

  /// <summary>
  /// Release the ownership claim to this list. Returns
  /// false if it wasn't a claimer in the first place.
  /// </summary>
  public bool ReleaseOwnerShip(ITileListOwner claimer)
  {
    if(claimer.ClaimTracker != this)
    {
      throw new InvalidOperationException(
        $"ITileListOwner.ClaimedTileListId does not match");
    }
    return _claimers.Remove(claimer);
  }

  /// <summary>
  /// The ITileListOwner that is currently the owner of this
  /// (or null if none)
  /// </summary>
  public ITileListOwner? Owner {
    get => _claimers.Count == 0 ? null : _claimers[0];
  }

  public bool HasOwner { get => _claimers.Count > 0; }

  public bool HasConflict { get => _claimers.Count > 1; }

  public IEnumerable<ITileListOwner> EnumClaimers()
  {
    return _claimers;
  }
}
