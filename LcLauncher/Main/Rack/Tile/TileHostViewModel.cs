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
    CopyMarkedTileHereCommand = new DelegateCommand(
      p => {
        if(CanCopyTileHere(Rack.KeyTile))
        {
          CopyTileHere(Rack.KeyTile!);
        }
      },
      p =>
        CanCopyTileHere(Rack.KeyTile));
    SwapMarkedTileHereCommand = new DelegateCommand(
      p => {
        if(CanSwapTileHere(Rack.KeyTile))
        {
          SwapTileHere(Rack.KeyTile!);
        }
      },
      p =>
        CanSwapTileHere(Rack.KeyTile));
    ConfirmAndClearTileCommand = new DelegateCommand(
      p => {
        var result = ConfirmAndClearTile();
      },
      p => !IsKeyTile);
  }

  public ICommand ToggleCutCommand { get; }

  public ICommand InsertEmptyTileCommand { get; }

  public ICommand CopyMarkedTileHereCommand { get; }

  public ICommand SwapMarkedTileHereCommand { get; }

  public ICommand ConfirmAndClearTileCommand { get; }

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

  public bool IsEmptyTile()
  {
    return Tile == null ||
      Tile is EmptyTileViewModel;
  }

  public bool IsGroupTile()
  {
    return Tile != null &&
      Tile is GroupTileViewModel;
  }

  public bool IsLaunchTile()
  {
    return Tile != null &&
      Tile is LaunchTileViewModel;
  }

  public bool IsQuadTile()
  {
    return Tile != null &&
      Tile is QuadTileViewModel;
  }

  public TileData? ReplaceTile(
    TileData? newRawData)
  {
    if(IsKeyTile)
    {
      throw new InvalidOperationException(
        "Cannot edit the marked tile");
    }
    var oldRawData = Tile?.GetModel();
    Tile = TileViewModel.Create(
      TileList,
      newRawData);
    TileList.MarkDirty();
    return oldRawData;
  }

  /// <summary>
  /// Clear this tile to an empty tile, destroying the original tile.
  /// </summary>
  public TileData? ClearTile()
  {
    return ReplaceTile(TileData.EmptyTile());
  }

  public TileData? ConfirmAndClearTile()
  {
    if(!IsKeyTile)
    {
      var result = MessageBox.Show(
        "Are you sure you want to delete this tile?\n(This cannot be undone)",
        "Delete Tile",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);
      if(result == MessageBoxResult.Yes)
      {
        return ClearTile();
      }
      else
      {
        // user cancelled
        return null;
      }
    }
    // refused
    return null;
  }

  /// <summary>
  /// Delete this tile from the list, destroying the original tile.
  /// Also pads the list to make the last row full again (i.e.: adds
  /// an empty tile at the end).
  /// </summary>
  public TileData? DeleteTile()
  {
    if(IsKeyTile)
    {
      throw new InvalidOperationException(
        "Cannot clear or delete the marked tile");
    }
    var oldData = Tile?.GetModel();
    Tile = null;
    TileList.Tiles.Remove(this);
    TileList.PadRow();
    TileList.MarkDirty();
    return oldData;
  }

  public bool CanDeleteTile()
  {
    return !IsKeyTile;
  }

  public TileData? CopyTileHere(
    TileHostViewModel other)
  {
    if(other == this)
    {
      throw new InvalidOperationException(
        "Cannot copy a tile to itself");
    }
    if(IsKeyTile)
    {
      throw new InvalidOperationException(
        "Cannot copy to the marked tile");
    }
    if(!IsEmpty)
    {
      throw new InvalidOperationException(
        "Cannot copy tile here, because this tile is not empty");
    }
    if(other.IsGroupTile())
    {
      // this would create a duplicate tile list reference
      throw new InvalidOperationException(
        "Cannot 'copy' group tiles");
    }
    if(other.IsKeyTile)
    {
      // expected to be the normal case
      other.IsKeyTile = false;
    }
    var newData = other.Tile?.GetModel();
    return ReplaceTile(newData);
  }

  public bool CanCopyTileHere(
    TileHostViewModel? other)
  {
    return
      other != null
      && other != this
      && !IsKeyTile
      && IsEmpty
      && !other.IsGroupTile();
  }

  public void SwapTileHere(
    TileHostViewModel other)
  {
    if(other == this)
    {
      throw new InvalidOperationException(
        "Cannot swap a tile with itself");
    }
    var otherData = other.Tile?.GetModel();
    other.IsKeyTile = false;
    IsKeyTile = false;
    var hereData = ReplaceTile(otherData);
    other.ReplaceTile(hereData);
  }

  public bool CanSwapTileHere(
    TileHostViewModel? other)
  {
    return
      other != null
      && other != this;
  }
}
