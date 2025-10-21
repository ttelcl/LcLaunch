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
/// Describes an icon to load from memory cache, disk cache,
/// or system, plus what to do with the result of that operation.
/// </summary>
public class IconJob : IIconJob
{
  /// <summary>
  /// Create a new <see cref="IconJob"/>
  /// </summary>
  /// <param name="target">
  /// The <see cref="IIconJobTarget"/> to update icons for
  /// </param>
  /// <param name="requestlevel">
  /// How deep to go to find the icon before bailing out.
  /// </param>
  /// <param name="refresh">
  /// If true, use the load level implied by <paramref name="requestLevel"/>
  /// without first trying the cheaper levels. This would for instance
  /// allow handling an updated icon.
  /// </param>
  public IconJob(
    IIconJobTarget target,
    IconLoadLevel requestLevel,
    bool refresh)
  {
    Target = target;
    RequestLevel = requestLevel;
    Refresh = refresh;
    // Take a snapshot of mutable parts of the targets
    IconSource = Target.IconSource;
    // Initial result: the unmodified input
    IconIdResult = Target.IconIds.Clone();
    IconResult = Target.Icons.Clone();
  }

  public IIconJobTarget Target { get; }

  public IconLoadLevel RequestLevel { get; private set; }

  public bool Refresh { get; private set; }

  /// <summary>
  /// The <see cref="Target"/>'s <see cref="IIconJobTarget.IconSource"/> at the
  /// time this job was created.
  /// </summary>
  public string IconSource { get; }

  public Guid TargetId => Target.IconTargetId;

  public IconSize IconSizes => Target.IconSizes;

  public bool Done { get; private set; }

  internal void PushResult()
  {
    if(!Done)
    {
      Done = true;
      Target.UpdateIcons(IconIdResult, IconResult);
    }
  }

  public IconIdSet IconIdResult { get; }

  public IconSet IconResult { get; }

  public bool IsMissingIcons()
  {
    return IconSizes.Unpack().Any(s => IconResult[s] == null);
  }

  public bool IsMissingIconIds()
  {
    return IconSizes.Unpack().Any(s => !IconIdResult[s].HasValue);
  }

  /// <summary>
  /// True if the Icon Source has changed since creating this job,
  /// making this job invalid
  /// </summary>
  public bool IsStale => IconSource != Target.IconSource;

  internal void MergeOlderJob(IconJob olderJob)
  {
    if(olderJob.RequestLevel > RequestLevel)
    {
      RequestLevel = olderJob.RequestLevel;
    }
    Refresh = Refresh || olderJob.Refresh;
  }
}
