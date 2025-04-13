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
    _title = Model.GetEffectiveTitle();
    _tooltip = Model.GetEffectiveTooltip();
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
  public ShellLaunch? ShellModel { get => Model as ShellLaunch; }

  /// <summary>
  /// The model for this tile, if it is a raw launch.
  /// Exactly one of <see cref="ShellModel"/> or
  /// <see cref="RawModel"/> is not null.
  /// </summary>
  public RawLaunch? RawModel { get => Model as RawLaunch; }

  public string Title {
    get => _title;
    set {
      if(SetValueProperty(ref _title, value))
      {
        // TODO: feed back to original and save
        Model.Title = value;
      }
    }
  }
  private string _title;

  public string Tooltip {
    get => _tooltip;
    set {
      if(SetValueProperty(ref _tooltip, value))
      {
        // TODO: feed back to original and save
        Model.Tooltip = value;
      }
    }
  }
  private string _tooltip;

  public override TileData? GetModel()
  {
    return Model switch {
      ShellLaunch shell => TileData.ShellTile(shell),
      RawLaunch raw => TileData.RawTile(raw),
      _ => throw new InvalidOperationException(
        $"Invalid launch data type {Model.GetType().FullName}")
    };
  }

  public string FallbackIcon => Model switch {
    ShellLaunch => "RocketLaunch",
    RawLaunch => "RocketLaunchOutline",
    _ => "Help"
  };
}
