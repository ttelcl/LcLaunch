/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

public class TileHostViewModel: ViewModelBase
{
  public TileHostViewModel(
    TileListViewModel tileList)
  {
    TileList = tileList;
  }

  public TileListViewModel TileList { get; }

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
      }
    }
  }
  private TileViewModel? _tile;

  public bool HasTile {
    get => _tile != null;
  }
}
