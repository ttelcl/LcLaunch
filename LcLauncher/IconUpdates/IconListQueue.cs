/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Main.Rack.Tile;
using LcLauncher.Persistence;

namespace LcLauncher.IconUpdates;

/// <summary>
/// Pending icon loads for one single tile list.
/// </summary>
public class IconListQueue
{
  private readonly Dictionary<Guid, IconLoadJob> _jobs;
  private readonly Dictionary<Guid, IPostIconLoadActor> _postLoadActors;

  /// <summary>
  /// Create a new IconListQueue
  /// </summary>
  public IconListQueue(
    IconLoadQueue parent,
    TileListViewModel target)
  {
    _jobs = [];
    _postLoadActors = [];
    // do not use target ID: the same list may be used in multiple view models!
    QueueId = Guid.NewGuid();
    TargetId = target.Model.Id;
    Parent = parent;
    Target = target;    
  }

  public Guid QueueId { get; }

  public Guid TargetId { get; }

  public IconLoadQueue Parent { get; }

  public IPersisted Target { get; }

  /// <summary>
  /// Called after all load jobs in this queue have been processed,
  /// and this list queue is about to be detached from the parent queue.
  /// </summary>
  internal void OnQueueCompleted()
  {
    Target.SaveIfDirty();
    var postLoadActors = _postLoadActors.Values.ToList();
    _postLoadActors.Clear();
    foreach(var actor in postLoadActors)
    {
      actor.PostIconLoad();
    }
  }

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

  public void QueuePostLoadActor(
    IPostIconLoadActor actor)
  {
    // Remove existing actor first, adding the new one at the end.
    _postLoadActors.Remove(actor.PostIconLoadId);
    _postLoadActors.Add(actor.PostIconLoadId, actor);
    // make sure this queue is active, even if there are no jobs for it
    Parent.ActivateListQueue(this);
  }

  public int JobCount()
  {
    return _jobs.Count;
  }
}
