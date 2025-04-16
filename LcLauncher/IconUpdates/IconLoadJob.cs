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
  public IconLoadJob(
    TileListViewModel saveTarget,
    IIconHost iconHost,
    Action load)
  {
    IconJobQueue = saveTarget.IconJobQueue;
    SaveTarget = saveTarget;
    IconHost = iconHost;
    Load = load;
  }

  public TileListViewModel SaveTarget { get; }

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
