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
  public EmptyTileViewModel(
    TileListViewModel ownerList,
    TileData? model,
    string? icon = null)
    : base(ownerList)
  {
    Model = model;
    Icon = icon ?? FindIcon();
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

  public string Icon {
    get => _icon;
    set {
      if(SetValueProperty(ref _icon, value))
      {
      }
    }
  }
  private string _icon = "Egg";

  public override string PlainIcon { get => Icon; }

  private string FindIcon()
  {
    return Model switch {
      null => "EggOutline",
      { ShellLaunch: { } } => "RocketLaunch",
      { RawLaunch: { } } => "RocketLaunchOutline",
      { Group: { } } => "DotsGrid",
      { Quad: { } } => "ViewGrid",
      _ => "Egg",
    };
  }
}
