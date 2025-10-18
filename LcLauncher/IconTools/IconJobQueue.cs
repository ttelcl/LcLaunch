/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcLauncher.IconTools;

/// <summary>
/// Tracks pending <see cref="IconJob"/>s, both per target and per source
/// </summary>
public class IconJobQueue
{
  private readonly object _lock = new(); // net8 compat: we don't have Lock class yet
  private readonly Dictionary<Guid, IconJob> _jobsPerTarget;
  private readonly Dictionary<string, Dictionary<Guid, IconJob>> _jobsPerSource;

  /// <summary>
  /// Create a new IconJobQueue
  /// </summary>
  public IconJobQueue()
  {
    _jobsPerTarget = [];
    _jobsPerSource = new Dictionary<string, Dictionary<Guid, IconJob>>(
      StringComparer.OrdinalIgnoreCase);
  }

  /// <summary>
  /// True if there are any waiting jobs. This may return true
  /// even if there is no work; call <see cref="DequeueBatch(int)"/>
  /// (or <see cref="Cleanup"/>) to make sure the value is right.
  /// </summary>
  public bool HasWork => _jobsPerSource.Count > 0;

  private volatile  bool _isCurrent;
  /// <summary>
  /// Set by the owner. Setting this to causes the current batch
  /// to be aborted at the next opportunity.
  /// </summary>
  public bool IsCurrent {
    get => _isCurrent;
    set => _isCurrent = value;
  }

  public void Enqueue(
    IIconJobTarget target,
    IconLoadLevel requestLevel,
    bool refresh)
  {
    Enqueue(new IconJob(target, requestLevel, refresh));
  }

  public void Enqueue(IconJob iconJob)
  {
    if(_jobsPerTarget.TryGetValue(iconJob.TargetId, out var existingJob))
    {
      // Resolve differences and update the subqueue
      // First remove the existing entry everywhere
      GetSourceJobs(existingJob.IconSource).Remove(existingJob.TargetId);
      _jobsPerTarget.Remove(iconJob.TargetId);
      // Then set the request level and refresh level to the maximum of them
      // for the two jobs.
      iconJob.MergeOlderJob(existingJob);
    }
    _jobsPerTarget[iconJob.TargetId] = iconJob;
    GetSourceJobs(iconJob.IconSource)[iconJob.TargetId] = iconJob;
  }

  /// <summary>
  /// Take up to <paramref name="maxSources"/> icon sources with pending
  /// jobs, remove them and their jobs from this queue, and return them.
  /// </summary>
  /// <param name="maxSources"></param>
  /// <returns></returns>
  public Dictionary<string, List<IconJob>> DequeueBatch(int maxSources)
  {
    Cleanup();
    var batch = new Dictionary<string, List<IconJob>>();
    // Gather the batch
    foreach(var subkvp in _jobsPerSource)
    {
      batch.Add(subkvp.Key, subkvp.Value.Values.ToList());
      if(batch.Count >= maxSources)
      {
        break;
      }
    }
    // Remove it
    foreach(var kvp in batch)
    {
      _jobsPerSource.Remove(kvp.Key);
      foreach(var job in kvp.Value)
      {
        _jobsPerTarget.Remove(job.TargetId);
      }
    }
    return batch;
  }

  /// <summary>
  /// Remove stale jobs and empty subqueues
  /// </summary>
  public void Cleanup()
  {
    var emptyIconSources = new List<string>();
    var staleJobs = new List<Guid>();
    foreach(var subkvp in _jobsPerSource)
    {
      var activeCount = 0;
      var staleJobs2 = new List<Guid>();
      foreach(var job in subkvp.Value.Values)
      {
        if(job.IsStale)
        {
          staleJobs.Add(job.TargetId);
          staleJobs2.Add(job.TargetId);
        }
        else
        {
          activeCount++;
        }
      }
      if(activeCount == 0)
      {
        emptyIconSources.Add(subkvp.Key);
        subkvp.Value.Clear();
      }
      else
      {
        foreach(var jobid in staleJobs2)
        {
          subkvp.Value.Remove(jobid);
        }
      }
    }
    foreach(var emptyIconSource in emptyIconSources)
    {
      _jobsPerSource.Remove(emptyIconSource);
    }
    foreach(var jobid in staleJobs)
    {
      _jobsPerTarget.Remove(jobid);
    }
  }

  private Dictionary<Guid, IconJob> GetSourceJobs(string iconSource)
  {
    if(!_jobsPerSource.TryGetValue(iconSource, out var sourceJobs))
    {
      sourceJobs = new Dictionary<Guid, IconJob>();
      _jobsPerSource[iconSource] = sourceJobs;
    }
    return sourceJobs;
  }
}
