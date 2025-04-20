/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
  }

  public ICommand ToggleCutCommand { get; }

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
      }
    }
  }
  private bool _isKeyTile = false;

  public int GetTileIndex()
  {
    return TileList.Tiles.IndexOf(this);
  }

  public override string ToString()
  {
    return $"{TileList.Model.Id}[{GetTileIndex()}]";
  }
}
