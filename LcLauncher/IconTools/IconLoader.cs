/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ttelcl.Persistence.API;

namespace LcLauncher.IconTools;

/// <summary>
/// A one stop shop for "loading" icons: both finding icons
/// from the wrapped cache by ID, maintaining that cache, and
/// converting "icon sources" (executables, documents, shell URIs,
/// etc.) into icons for caching. Icon loading happens in batches
/// of IconJobs.
/// </summary>
public class IconLoader
{
  private readonly object _lock = new();
  private volatile bool _busy;

  /// <summary>
  /// Create a new IconLoader
  /// </summary>
  public IconLoader(
    IBlobBucket cacheBucket)
  {
    IconCache = new IconCache(cacheBucket);
  }

  /// <summary>
  /// The embedded icon cache
  /// </summary>
  public IconCache IconCache { get; }

  public IBlobBucket CacheBucket => IconCache.BlobBucket;

  /// <summary>
  /// Try starting the processing the next batch of icon load jobs.
  /// Returns false if there are no more jobs and all jobs have been
  /// processed. (In the async version those two conditions are not the same)
  /// </summary>
  /// <param name="jobQueue"></param>
  /// <param name="batchSize"></param>
  /// <returns></returns>
  public bool ProcessNextBatch(IconJobQueue jobQueue, int batchSize)
  {
    if(!jobQueue.IsCurrent)
    {
      // caught in the middle of aborting and canceling
      return false; 
    }
    lock(_lock)
    {
      if(_busy)
      {
        // still working on the previous batch
        return false;
      }
      _busy = true;
    }
    try
    {
      // Jobs that definitely require writing the disk cache
      var diskWriteJobs = new List<IconJob>();
      // Jobs that may or may not require writing the disk cache
      var maybeDiskWriteJobs = new List<IconJob>();
      //// Finished jobs
      //var finishedJobs = new List<IconJob>();

      var batch = jobQueue.DequeueBatch(batchSize);
      // first flatten the batch into a list of jobs
      var jobs = batch.Values.SelectMany(iconjobs => iconjobs);
      // Split the jobs into categories that require different ways of handling
      foreach(var job in jobs)
      {
        switch(job.RequestLevel)
        {
          case IconLoadLevel.MemoryCache:
          case IconLoadLevel.DiskCache:
            maybeDiskWriteJobs.Add(job);
            break;
          case IconLoadLevel.System:
            if(job.Refresh)
            {
              diskWriteJobs.Add(job);
            }
            else
            {
              if(job.IsMissingIconIds())
              {
                // Cheaper things than system icon generation won't work anyway,
                // so skip straight to hard loading.
                diskWriteJobs.Add(job);
              }
              else
              {
                maybeDiskWriteJobs.Add(job);
              }
            }
            break;
          default:
            Trace.TraceError(
              $"unrecognized Icon Job request level: {job.RequestLevel}. Ignoring job.");
            break;
        }
      }
      if(maybeDiskWriteJobs.Count > 0)
      {
        // First handle cases that may require no more than *reading* from the disk cache
        // This may include cases that may get promoted to the "needs writing" group.
        // Those will be added to diskWriteJobs
        using var reader = IconCache.StartReader();
        // No use in grouping by source here.
        // No use in grouping by ID either - that's the whole reason the memory cache exists
        foreach(var job in maybeDiskWriteJobs)
        {
          switch(job.RequestLevel)
          {
            case IconLoadLevel.MemoryCache:
              foreach(var size in job.IconSizes.Unpack())
              {
                if(job.IconResult[size] == null || job.Refresh)
                {
                  var icon = reader.FindIcon(
                    job.IconIdResult[size],
                    IconCacheLoadLevel.FromCache);
                  job.IconResult[size] = icon;
                }
              }
              //finishedJobs.Add(job);
              job.PushResult();
              break;
            case IconLoadLevel.DiskCache:
              foreach(var size in job.IconSizes.Unpack())
              {
                if(job.IconResult[size] == null || job.Refresh)
                {
                  var icon = reader.FindIcon(
                      job.IconIdResult[size],
                      job.Refresh
                      ? IconCacheLoadLevel.LoadAlways
                      : IconCacheLoadLevel.LoadIfMissing);
                  job.IconResult[size] = icon;
                }
              }
              //finishedJobs.Add(job);
              job.PushResult();
              break;
            case IconLoadLevel.System:
              // First do the same as IconLoadLevel.DiskCache
              foreach(var size in job.IconSizes.Unpack())
              {
                if(job.IconResult[size] == null)
                {
                  // We know Refresh == false because otherwise it would be
                  // in diskWriteJobs already.
                  var icon = reader.FindIcon(
                      job.IconIdResult[size],
                      IconCacheLoadLevel.LoadIfMissing);
                  job.IconResult[size] = icon;
                }
              }
              if(job.IsMissingIcons())
              {
                // Still missing icons. Upgrade this job to system icon refresh update
                diskWriteJobs.Add(job);
              }
              else
              {
                //finishedJobs.Add(job);
                job.PushResult();
              }
              break;
          }
        }
      }
      if(diskWriteJobs.Count > 0)
      {
        using var writer = IconCache.StartWriter();
        // Process source-by-source
        var bySource = diskWriteJobs.GroupBy(job => job.IconSource);
        foreach(var jobGroup in bySource)
        {
          var source = jobGroup.Key;
          var iconSizes = IconSize.None;
          foreach(var job in jobGroup)
          {
            iconSizes |= job.IconSizes;
          }
          var icons = IconExtraction.IconsForSource(source, iconSizes);
          if(icons == null)
          {
            Trace.TraceError(
              $"Failed to get icons for '{source}'");
            continue;
          }
          foreach(var job in jobGroup)
          {
            PushToJob(job, icons, writer);
            //finishedJobs.Add(job);
            job.PushResult();
          }
        }

      }

      //foreach(var job in finishedJobs)
      //{
      //  job.PushResult();
      //}

      bool hasWork = jobQueue.HasWork;
      return hasWork;
    }
    finally
    {
      lock(_lock)
      {
        _busy = false;
      }
    }
  }

  private static void PushToJob(
    IIconJob job,
    IconSet icons,
    IconCache.Writer writer)
  {
    foreach(var size in job.IconSizes.Unpack())
    {
      var icon = icons[size];
      if(icon == null)
      {
        // should never happen
        continue;
      }
      var hashId = writer.PutIcon(icon);
      job.IconIdResult[size] = hashId;
      job.IconResult[size] = icon;
    }
  }

  /// <summary>
  /// Pushes the retrieved icons to the Icon store and to the job.
  /// This WILL open a new writer on the cache, so can not be executed
  /// if another writer or a reader is active on it.
  /// </summary>
  /// <param name="job"></param>
  /// <param name="icons"></param>
  public void PushToJob(
    IIconJob job,
    IconSet icons)
  {
    using var writer = IconCache.StartWriter();
    PushToJob(job, icons, writer);
  }
}
