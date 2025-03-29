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

namespace LcLauncher.Main;

/// <summary>
/// Dummy view model for an empty tile.
/// </summary>
public class EmptyTileViewModel: TileViewModelBase
{
  public EmptyTileViewModel()
  {
  }

  public override TileData GetModel()
  {
    return TileData.EmptyTile();
  }
}
