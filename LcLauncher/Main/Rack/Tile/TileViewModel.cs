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

namespace LcLauncher.Main.Rack.Tile;

public abstract class TileViewModel: ViewModelBase
{
  protected TileViewModel()
  {
  }

  public static TileViewModel Create(TileData? model)
  {
    return model switch {
      null => new EmptyTileViewModel(null),
      { ShellLaunch: { } shellLaunch } => LaunchTileViewModel.FromShell(shellLaunch),
      { RawLaunch: { } rawLaunch } => LaunchTileViewModel.FromRaw(rawLaunch),
      //{ Quad: { } quadTile } => new QuadTileViewModel(quadTile),
      //{ Group: { } group } => new GroupTileViewModel(group),
      _ => new EmptyTileViewModel(model)
    };
  }

  public TileHostViewModel? Host {
    get => _host;
    internal set {
      if(SetNullableInstanceProperty(ref _host, value))
      {
      }
    }
  }
  private TileHostViewModel? _host;

  /// <summary>
  /// Get a JSON-serializable model for this tile
  /// view model. This may be the original model the tile
  /// view model was created from, or a new model.
  /// In the case of an empty tile, this may be null.
  /// </summary>
  public abstract TileData? GetModel();
}
