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

using Model2 = LcLauncher.ModelsV2;
using Model3 = LcLauncher.DataModel.Entities;

namespace LcLauncher.Main.Rack.Tile;

public class QuadTileViewModel: TileViewModel
{
  public QuadTileViewModel(
    TileListViewModel ownerList,
    IEnumerable<Model2.LaunchData?> model)
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

  public List<Model2.LaunchData?> RawModel { get; }

  public override string PlainIcon { get => "ViewGrid"; }

  public override Model2.TileData? GetModel()
  {
    return Model2.TileData.QuadTile(
      RawModel);
  }
}
