/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

public class TileHostViewModel: ViewModelBase
{
  public TileHostViewModel(
    TileListViewModel tileList)
  {
    TileList = tileList;
    ToggleCutCommand = new DelegateCommand(
      p => {
        IsKeyTile = Rack.KeyTile != this;
      });
    InsertEmptyTileCommand = new DelegateCommand(
      p => TileList.InsertEmptyTile(this));
  }

  public ICommand ToggleCutCommand { get; }

  public ICommand InsertEmptyTileCommand { get; }

  public TileListViewModel TileList { get; }

  public ShelfViewModel Shelf => TileList.Shelf;

  public RackViewModel Rack => TileList.Shelf.Rack;

  public TileViewModel? Tile {
    get => _tile;
    set {
      var oldTile = _tile;
      if(SetNullableInstanceProperty(ref _tile, value))
      {
        if(oldTile != null)
        {
          oldTile.Host = null;
        }
        if(value != null)
        {
          value.Host = this;
        }
        RaisePropertyChanged(nameof(HasTile));
        RaisePropertyChanged(nameof(IsEmpty));
      }
    }
  }
  private TileViewModel? _tile;

  public bool HasTile {
    get => _tile != null;
  }

  public bool IsEmpty {
    get => _tile == null || _tile is EmptyTileViewModel;  
  }

  public bool Hovering {
    get => _hovering;
    set {
      if(SetValueProperty(ref _hovering, value))
      {
        RaisePropertyChanged(nameof(Hovering));
      }
    }
  }
  private bool _hovering = false;

  public bool IsKeyTile {
    get => _isKeyTile;
    set {
      if(SetValueProperty(ref _isKeyTile, value))
      {
        if(_isKeyTile)
        {
          // Note: Make sure this doesn't recurse indefinitely
          // Setting Rack.KeyTile to this will call this property
          // setter, but the 'ifs' above will block further recursion.
          Rack.KeyTile = this;
        }
        else if(Rack.KeyTile == this)
        {
          Rack.KeyTile = null;
        } // else: don't affect Rack.KeyTile
        RaisePropertyChanged(nameof(MarkTileText));
      }
    }
  }
  private bool _isKeyTile = false;

  public string MarkTileText { get => IsKeyTile ? "Unmark Tile" : "Mark Tile"; }

  public int GetTileIndex()
  {
    return TileList.Tiles.IndexOf(this);
  }

  public override string ToString()
  {
    return $"{TileList.Model.Id}[{GetTileIndex()}]";
  }

  /// <summary>
  /// Clear this tile to an empty tile, destroying the original tile.
  /// </summary>
  public void ClearTile()
  {
    if(IsKeyTile)
    {
      throw new InvalidOperationException(
        "Cannot clear or delete the marked tile");
    }
    Tile = new EmptyTileViewModel(TileList, TileData.EmptyTile());
    TileList.RebuildModel(); // includes MarkDirty()
  }

  /// <summary>
  /// Delete this tile from the list, destroying the original tile.
  /// Also pads the list to make the last row full again (i.e.: adds
  /// an empty tile at the end).
  /// </summary>
  public void DeleteTile()
  {
    if(IsKeyTile)
    {
      throw new InvalidOperationException(
        "Cannot clear or delete the marked tile");
    }
    TileList.Tiles.Remove(this);
    TileList.PadRow();
    TileList.RebuildModel(); // includes MarkDirty()
  }

  public void CopyTileHere(
    TileHostViewModel other)
  {
    if(IsKeyTile)
    {
      throw new InvalidOperationException(
        "Cannot target the marked tile");
    }
    if(!IsEmpty)
    {
      throw new InvalidOperationException(
        "Cannot copy tile here, because this tile is not empty");
    }
    var tile = TileViewModel.Create(
      TileList,
      other.Tile?.GetModel());
    Tile = tile;
    TileList.MarkDirty();
    if(other.IsKeyTile)
    {
      // expected to be the normal case
      other.IsKeyTile = false;
    }
  }

  public void SwapTileHere(
    TileHostViewModel other)
  {
    if(IsKeyTile)
    {
      throw new InvalidOperationException(
        "Cannot target the marked tile");
    }
    if(!IsEmpty)
    {
      throw new InvalidOperationException(
        "Cannot swap tile here, because this tile is not empty");
    }
    var tile = TileViewModel.Create(
      TileList,
      other.Tile?.GetModel());
    Tile = tile;
    TileList.MarkDirty();
    if(other.IsKeyTile)
    {
      // expected to be the normal case
      other.IsKeyTile = false;
    }
    other.ClearTile();
  }
}
