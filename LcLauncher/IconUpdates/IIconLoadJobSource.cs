/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.IconUpdates;

/// <summary>
/// An object that can provide a sequence of icon load jobs.
/// </summary>
public interface IIconLoadJobSource
{
  IEnumerable<IconLoadJob> GetIconLoadJobs(bool reload);

  IconLoadQueue IconLoadQueue { get; }
}

public static class IconLoadJobSourceExtensions
{
  public static void EnqueueAllIconJobs(
    this IIconLoadJobSource source, bool reload)
  {
    source.IconLoadQueue.EnqueueAll(source, reload);
  }
}

