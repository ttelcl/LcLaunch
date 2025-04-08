/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LcLauncher.Models;

namespace LcLauncher.Main.Rack.Tile;

public class EmptyTileViewModel: TileViewModel
{
  public EmptyTileViewModel(TileData? model)
  {
    Model=model;
  }

  /// <summary>
  /// The original model this tile view model was created from,
  /// possibly null. Unlike other tiles, this is immutable.
  /// </summary>
  public TileData? Model { get; }

  public override TileData? GetModel()
  {
    return TileData.EmptyTile();
  }
}
