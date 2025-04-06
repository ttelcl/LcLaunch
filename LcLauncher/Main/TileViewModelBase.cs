/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main;

public abstract class TileViewModelBase: ViewModelBase
{
  protected TileViewModelBase()
  {
  }

  public static TileViewModelBase Create(TileData? model)
  {
    return model switch {
      null => new EmptyTileViewModel(),
      { ShellLaunch: { } shellLaunch } => new ShellTileViewModel(shellLaunch),
      { RawLaunch: { } rawLaunch } => new RawTileViewModel(rawLaunch),
      { Quad: { } quadTile } => new QuadTileViewModel(quadTile),
      { Group: { } group } => new GroupTileViewModel(group),
      _ => new EmptyTileViewModel()
    };
  }

  /// <summary>
  /// Get the JSON-serializable model for this tile
  /// view model.
  /// </summary>
  public abstract TileData GetModel();
}
