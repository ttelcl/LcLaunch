/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcLauncher.Main.Rack.Tile;


public class IconLoadJob
{
  public IconLoadJob(
    TileListViewModel saveTarget,
    Action load)
  {
    SaveTarget = saveTarget;
    Load = load;
  }

  public TileListViewModel SaveTarget { get; }

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

  public void CheckSave()
  {
    if(SaveTarget.IsDirty)
    {
      Trace.TraceInformation(
        $"Saving tile list {SaveTarget.Model.Id}");
      SaveTarget.RebuildModel();
      SaveTarget.SaveRaw();
    }
  }
}
