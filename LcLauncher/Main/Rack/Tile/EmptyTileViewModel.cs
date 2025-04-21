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
using System.Windows;
using System.Windows.Input;
using LcLauncher.WpfUtilities;

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
    DeleteEmptyTileCommand = new DelegateCommand(
      p => DeleteTile(),
      p => CanDeleteTile());
    CopyKeyTileHereCommand = new DelegateCommand(
      p => CopyKeyTileHere(),
      p => CanCopyKeyTileHere());
    SwapKeyTileHereCommand = new DelegateCommand(
      p => SwapKeyTileHere(),
      p => CanSwapKeyTileHere());
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

  public ICommand DeleteEmptyTileCommand { get; }

  private void DeleteTile()
  {
    if(Host != null)
    {
      Host.DeleteTile();
    }
  }

  private bool CanDeleteTile()
  {
    return Host != null && !Host.IsKeyTile;
  }

  public ICommand CopyKeyTileHereCommand { get; }

  private void CopyKeyTileHere()
  {
    if(Host != null && Host.Rack.KeyTile != null)
    {
      Host.CopyTileHere(Host.Rack.KeyTile);
    }
  }

  private bool CanCopyKeyTileHere()
  {
    return Host != null && Host.Rack.KeyTile != null && !Host.IsKeyTile;
  }

  public ICommand SwapKeyTileHereCommand { get; }

  private void SwapKeyTileHere()
  {
    if(Host != null && Host.Rack.KeyTile != null)
    {
      Host.SwapTileHere(Host.Rack.KeyTile);
    }
  }

  private bool CanSwapKeyTileHere()
  {
    return Host != null && Host.Rack.KeyTile != null && !Host.IsKeyTile;
  }
}
