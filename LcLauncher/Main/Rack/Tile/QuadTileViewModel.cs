/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Models;

namespace LcLauncher.Main.Rack.Tile;

public class QuadTileViewModel: TileViewModel
{
  public QuadTileViewModel(
    TileListViewModel ownerList,
    IEnumerable<LaunchTile> model)
    : base(ownerList)
  {
    var rawModel = model.ToList();
    while(rawModel.Count < 4)
    {
      rawModel.Add(new LaunchTile(null, null));
    }
    // silently ignore excess tiles
    RawModel = rawModel;
    // Todo: create new viewmodels for the sub-tiles
  }

  public List<LaunchTile> RawModel { get; }

  public override TileData? GetModel()
  {
    return TileData.QuadTile(
      RawModel);
  }
}
