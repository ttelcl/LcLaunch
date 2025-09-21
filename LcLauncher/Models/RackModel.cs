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

using Newtonsoft.Json;

using LcLauncher.Storage;
using LcLauncher.Persistence;

using Model2 = LcLauncher.ModelsV2;
using Model3 = LcLauncher.DataModel.Entities;

namespace LcLauncher.Models;

/// <summary>
/// Manages one Rack database (all related files).
/// </summary>
public class RackModel
{
  private Dictionary<Guid, ShelfModel> _shelves;
  private readonly Dictionary<Guid, WeakReference<TileListOwnerTracker>>
    _claimTrackerCache;

  /// <summary>
  /// Create a new RackModel. If the name is unknown, it will be created.
  /// </summary>
  public RackModel(
    ILcLaunchStore store,
    string rackName)
  {
    _claimTrackerCache = [];
    LcLaunchStore.ValidateRackName(rackName);
    Store = store;
    RackName = rackName;
    _shelves = [];
    Columns = new List<ShelfModel>[3];
    var rackData = Store.LoadOrCreateRack(RackName);
    RackData = rackData;
    // populate shelves in the lookup and the columns
    for(var icol=0; icol<3; icol++)
    {
      Columns[icol] = [];
      var column = rackData.Columns[icol];
      foreach(var shelfId in column)
      {
        if(!_shelves.TryGetValue(shelfId, out var shelf))
        {
          shelf = ShelfModel.Load(this, shelfId);
          _shelves.Add(shelfId, shelf);
        }
        Columns[icol].Add(shelf);
      }
    }
  }

  public ILcLaunchStore Store { get; }

  /// <summary>
  /// The name of the rack (file name without path nor extension).
  /// </summary>
  public string RackName { get; }

  public Model2.RackData RackData { get; }

  public IReadOnlyDictionary<Guid, ShelfModel> ShelfMap => _shelves;

  /// <summary>
  /// The 3 columns of the rack.
  /// </summary>
  public List<ShelfModel>[] Columns { get; }

  /// <summary>
  /// Save this rack itself (the column setup).
  /// Does NOT save the shelves
  /// </summary>
  public void Save()
  {
    Store.SaveRack(RackName, RackData);
    IsDirty = false;
  }

  public bool IsDirty { get; private set; }

  public void MarkDirty()
  {
    IsDirty = true;
  }

  internal void RebuildRackData()
  {
    // Note: the outer and inner lists may also be referenced elsewhere,
    // so we can only replace their contents, not the lists themselves.

    if(Columns.Length != RackData.Columns.Count)
    {
      // midway through refactoring this ... this method is backcompat
      throw new NotSupportedException(
        $"Column count not synchronized: {Columns.Length} vs {RackData.Columns.Count}");
    }

    for(var i=0; i<Columns.Length; i++)
    {
      RackData.Columns[i].Clear();
      RackData.Columns[i].AddRange(Columns[i].Select(s => s.Id));
    }
  }

  public TileListOwnerTracker GetClaimTracker(Guid id)
  {
    if(_claimTrackerCache.TryGetValue(id, out var weakRef))
    {
      if(weakRef.TryGetTarget(out var tracker))
      {
        return tracker;
      }
    }
    var newTracker = TileListOwnerTracker.FromRack(this, id);
    _claimTrackerCache[id] = new WeakReference<TileListOwnerTracker>(newTracker);
    return newTracker;
  }

  public bool HasClaimTracker(Guid id)
  {
    return _claimTrackerCache.ContainsKey(id);
  }

  public IEnumerable<ITileListOwner> EnumClaims()
  {
    foreach(var tracker in EnumClaimTrackers())
    {
      foreach(var claim in tracker.EnumClaimers())
      {
        yield return claim;
      }
    }
  }

  /// <summary>
  /// Enumerate all claim trackers that have an owner.
  /// </summary>
  public IEnumerable<TileListOwnerTracker> EnumClaimTrackers()
  {
    foreach(var weakRef in _claimTrackerCache.Values)
    {
      if(weakRef.TryGetTarget(out var tracker))
      {
        if(tracker.HasOwner)
        {
          yield return tracker;
        }
      }
    }
  }

  public void TraceClaimStatus()
  {
    foreach(var tracker in EnumClaimTrackers())
    {
      if(tracker.HasConflict)
      {
        Trace.TraceError(
          $"List {tracker.TileListId} has ownership conflicts:");
        foreach(var claim in tracker.EnumClaimers())
        {
          if(claim.OwnsTileList())
          {
            Trace.TraceInformation(
              $"  Owner:    {claim.TileListOwnerLabel}");
          }
          else
          {
            Trace.TraceWarning(
              $"  Conflict: {claim.TileListOwnerLabel}");
          }
        }
      }
      else if(tracker.HasOwner)
      {
        Trace.TraceInformation(
          $"List {tracker.TileListId}: no conflict. Owned by {tracker.Owner!.TileListOwnerLabel}");
      }
      else
      {
        Trace.TraceWarning(
          $"Claim {tracker.TileListId} (no owner claims)");
      }
    }
  }
}
