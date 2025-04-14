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
    SaveTarget = saveTarget;
    IconHost = iconHost;
    Load = load;
  }

  public TileListViewModel SaveTarget { get; }

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

  //public void CheckSave()
  //{
  //  if(SaveTarget.IsDirty)
  //  {
  //    Trace.TraceInformation(
  //      $"Saving tile list {SaveTarget.Model.Id}");
  //    SaveTarget.RebuildModel();
  //    SaveTarget.SaveRaw();
  //  }
  //}
}
