/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

public class TileListViewModel: ViewModelBase
{
  public TileListViewModel(
    ShelfViewModel shelf,
    TileListModel model)
  {
    Shelf = shelf;
    Model = model;
    Tiles = new ObservableCollection<TileHostViewModel>();
    foreach(var tile in model.Tiles)
    {
      var host = new TileHostViewModel(this);
      var tileVm = TileViewModel.Create(tile);
      host.Tile = tileVm;
      Tiles.Add(host);
    }
    // TODO: padding; row/column model; the code below is temporary
    var rows = (Tiles.Count + 3) / 4;
    if(rows < 1)
    {
      rows = 1;
    }
    var expectedTileCount = rows * 4;
    while(Tiles.Count < expectedTileCount)
    {
      var host = new TileHostViewModel(this);
      host.Tile = new EmptyTileViewModel(null);
      Tiles.Add(host);
    }
  }

  public ShelfViewModel Shelf { get; }

  public TileListModel Model { get; }

  public bool IsPrimary { get => Model.Id == Shelf.Model.Id; }

  public ObservableCollection<TileHostViewModel> Tiles { get; }
}
