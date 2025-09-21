/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.ModelsV2;

namespace LcLauncher.Main.Rack.Tile;

public class QuadTileViewModel: TileViewModel
{
  public QuadTileViewModel(
    TileListViewModel ownerList,
    IEnumerable<LaunchData?> model)
    : base(ownerList)
  {
    var rawModel = model.ToList();
    while(rawModel.Count < 4)
    {
      rawModel.Add(null);
    }
    for(var i = 0; i < rawModel.Count; i++)
    {
      if(rawModel[i] is null)
      {
        continue;
      }
      if(String.IsNullOrEmpty(rawModel[i]?.Target))
      {
        // Fix misconversions from older versions
        rawModel[i] = null;
      }
    }
    // silently ignore excess tiles
    RawModel = rawModel;
    // Todo: create new viewmodels for the sub-tiles
  }

  public List<LaunchData?> RawModel { get; }

  public override string PlainIcon { get => "ViewGrid"; }

  public override TileData? GetModel()
  {
    return TileData.QuadTile(
      RawModel);
  }
}
