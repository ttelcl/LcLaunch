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
using LcLauncher.WpfUtilities;
using LcLauncher.Main.Rack.Tile;

namespace LcLauncher.Main.Rack;

public class ShelfViewModel: ViewModelBase
{
  public ShelfViewModel(
    ColumnViewModel column,
    ShelfModel model)
  {
    Column = column;
    Model = model;
    PrimaryTiles = new TileListViewModel(this, model.PrimaryTiles);
  }

  public ColumnViewModel Column { get; }

  public ShelfModel Model { get; }

  public TileListViewModel PrimaryTiles { get; }

  public TileListViewModel? SecondaryTiles {
    get => _secondaryTiles;
    set {
      if(SetNullableInstanceProperty(ref _secondaryTiles, value))
      {
        RaisePropertyChanged(nameof(HasSecondaryTiles));
      }
    }
  }
  private TileListViewModel? _secondaryTiles;

  public bool HasSecondaryTiles {
    get => _secondaryTiles != null;
  }
}
