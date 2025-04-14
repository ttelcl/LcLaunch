/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Main.Rack.Tile;

namespace LcLauncher.IconUpdates;

/// <summary>
/// Pending icon loads for one single tile list.
/// </summary>
public class IconListQueue
{
  private readonly Dictionary<Guid, IconLoadJob> _jobs;

  /// <summary>
  /// Create a new IconListQueue
  /// </summary>
  public IconListQueue(
    IconLoadQueue parent,
    TileListViewModel target)
  {
    _jobs = [];
    // do not use target ID: the same list may be used in multiple view models!
    QueueId = Guid.NewGuid();
    Parent = parent;
    Target = target;    
  }

  public Guid QueueId { get; }

  public IconLoadQueue Parent { get; }

  public TileListViewModel Target { get; }

  // Called from IconLoadQueue
  internal IconLoadJob? TryDequeueJob()
  {
    if(_jobs.Count == 0)
    {
      return null;
    }
    var kvp = _jobs.First();
    _jobs.Remove(kvp.Key);
    return kvp.Value;
  }

  // Called from IconLoadQueue
  internal void EnqueueJob(
    IconLoadJob job)
  {
    // Don't care if this replaces an existing job!
    _jobs[job.IconHost.IconHostId] = job;
  }

  public int JobCount()
  {
    return _jobs.Count;
  }
}
