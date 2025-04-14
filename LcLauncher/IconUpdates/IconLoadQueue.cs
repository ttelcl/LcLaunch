/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Main.Rack.Tile;

namespace LcLauncher.IconUpdates;

public class IconLoadQueue
{
  private readonly Dictionary<Guid, IconListQueue> _queues;
  private readonly Action<IconLoadQueue>? _activate;

  public IconLoadQueue(
    Action<IconLoadQueue>? activate)
  {
    _queues = [];
    _activate = activate ;
  }

  public IconListQueue? GetNextQueue()
  {
    if(_queues.Count == 0)
    {
      return null;
    }
    var kvp = _queues.First();
    return kvp.Value;
  }

  /// <summary>
  /// Process next job in the queue, returning true if there was a job,
  /// and false if the queue is empty.
  /// </summary>
  /// <returns></returns>
  public bool ProcessNextJob()
  {
    var queue = GetNextQueue();
    if(queue == null)
    {
      return false;
    }
    var job = queue.TryDequeueJob();
    if(job == null)
    {
      Trace.TraceInformation(
        $"Processed last icon load job for {queue.Target.Model.Id}");
      // No more jobs in this queue
      _queues.Remove(queue.QueueId);
      var target = queue.Target;
      if(target.IsDirty)
      {
        Trace.TraceInformation(
          $"Saving tile list {queue.Target.Model.Id}");
        target.RebuildModel();
        target.SaveRaw();
      }
    }
    else
    {
      Trace.TraceInformation(
        $"Processing icon load job for {job.SaveTarget.Model.Id}");
      job.Execute();
    }
    return true;
  }

  public void EnqueueJob(IconLoadJob job)
  {
    var wasEmpty = IsEmpty();
    if(!_queues.TryGetValue(job.SaveTarget.IconJobQueue.QueueId, out var queue))
    {
      // (Re)Insert the queue for this job into this master queue
      queue = job.SaveTarget.IconJobQueue;
      _queues[queue.QueueId] = queue;
    }
    queue.EnqueueJob(job);
    if(wasEmpty)
    {
      _activate?.Invoke(this);
    }
  }

  public void EnqueueAll(IIconLoadJobSource source, bool reload)
  {
    foreach(var job in source.GetIconLoadJobs(reload))
    {
      EnqueueJob(job);
    }
  }

  public bool IsEmpty()
  {
    return _queues.Count == 0;
  }

  public int JobCount()
  {
    var count = 0;
    foreach(var queue in _queues.Values)
    {
      count += queue.JobCount();
    }
    return count;
  }
}
