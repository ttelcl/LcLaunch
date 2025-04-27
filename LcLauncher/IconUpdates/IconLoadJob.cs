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

public class IconLoadJob
{
  /// <summary>
  /// Create a new icon load job.
  /// </summary>
  /// <param name="iconJobQueue">
  /// The inner queue to use for this job. This is the 
  /// <see cref="IconListQueue"/> instance associated
  /// with the target icon list.
  /// </param>
  /// <param name="iconHost">
  /// The Icon Host uniquely identifying the viewmodel for the
  /// control that will display the icon(s).
  /// </param>
  /// <param name="load">
  /// The action that will perform the actual loading of the icon.
  /// </param>
  public IconLoadJob(
    IconListQueue iconJobQueue,
    IIconHost iconHost,
    Action load)
  {
    IconJobQueue = iconJobQueue;
    IconHost = iconHost;
    Load = load;
  }

  public IconLoadJob(
    TileListViewModel saveTarget,
    IIconHost iconHost,
    Action load)
    : this(saveTarget.IconJobQueue, iconHost, load)
  {
  }

  public IconListQueue IconJobQueue { get; }

  /// <summary>
  /// The ID of the target icon list. Only for debugging.
  /// </summary>
  public Guid TargetId => IconJobQueue.TargetId;

  /// <summary>
  /// The ID of the queue this job is in, identifying the view model
  /// holding the list. A Queue ID implies a target ID, but not vice versa.
  /// </summary>
  public Guid QueueId => IconJobQueue.QueueId;

  public IIconHost IconHost { get; }

  public Action Load { get; }

  public void Execute()
  {
    try
    {
      Load();
    }
    catch(Exception ex)
    {
      Trace.TraceError(
        $"Error loading icon: {ex}");
    }
  }
}
