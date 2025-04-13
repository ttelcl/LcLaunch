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

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// Shared tile view model for both launch tiles.
/// </summary>
public class LaunchTileViewModel: TileViewModel
{
  private LaunchTileViewModel(
    LaunchData model)
  {
    Model = model;
    if(model is ShellLaunch shell)
    {
      ShellModel = shell;
      RawModel = null;
    }
    else if(model is RawLaunch raw)
    {
      ShellModel = null;
      RawModel = raw;
    }
    else
    {
      throw new InvalidOperationException(
        $"Invalid launch data type {model.GetType().FullName}");
    }
  }

  public static LaunchTileViewModel FromShell(
    ShellLaunch model)
  {
    return new LaunchTileViewModel(model);
  }

  public static LaunchTileViewModel FromRaw(
    RawLaunch model)
  {
    return new LaunchTileViewModel(model);
  }

  /// <summary>
  /// The model for this tile. This is either equal to
  /// <see cref="ShellModel"/> or <see cref="RawModel"/>
  /// (the one that is not null).
  /// </summary>
  public LaunchData Model { get; }

  /// <summary>
  /// The model for this tile, if it is a shell launch.
  /// Exactly one of <see cref="ShellModel"/> or 
  /// <see cref="RawModel"/> is not null.
  /// </summary>
  public ShellLaunch? ShellModel { get; }

  /// <summary>
  /// The model for this tile, if it is a raw launch.
  /// Exactly one of <see cref="ShellModel"/> or
  /// <see cref="RawModel"/> is not null.
  /// </summary>
  public RawLaunch? RawModel { get; }

  public override TileData? GetModel()
  {
    return Model switch {
      ShellLaunch shell => TileData.ShellTile(shell),
      RawLaunch raw => TileData.RawTile(raw),
      _ => throw new InvalidOperationException(
        $"Invalid launch data type {Model.GetType().FullName}")
    };
  }
}
