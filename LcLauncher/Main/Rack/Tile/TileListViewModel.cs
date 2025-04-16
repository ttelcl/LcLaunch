﻿/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.IconUpdates;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

public class TileListViewModel: ViewModelBase, IIconLoadJobSource
{

  public TileListViewModel(
    IconLoadQueue iconLoadQueue,
    ShelfViewModel shelf,
    TileListModel model)
  {
    Shelf = shelf;
    Model = model;
    Tiles = new ObservableCollection<TileHostViewModel>();
    IconJobQueue = new IconListQueue(iconLoadQueue, this);
    foreach(var tile in model.RawTiles)
    {
      var host = new TileHostViewModel(this);
      var tileVm = TileViewModel.Create(this, tile);
      host.Tile = tileVm;
      Tiles.Add(host);
    }
    PadRow();
  }

  public ShelfViewModel Shelf { get; }

  public TileListModel Model { get; }

  public ILauncherIconCache IconCache => Model.IconCache;

  public bool IsPrimary { get => Model.Id == Shelf.Model.Id; }

  public ObservableCollection<TileHostViewModel> Tiles { get; }

  public IconListQueue IconJobQueue { get; }

  /// <summary>
  /// Rebuild the persisted model from the viewmodels
  /// </summary>
  public void RebuildModel()
  {
    var newTiles = new List<TileData?>();
    foreach(var tile in Tiles)
    {
      newTiles.Add(tile?.Tile?.GetModel());
    }
    Model.RawTiles.Clear();
    Model.RawTiles.AddRange(newTiles);
    Model.MarkDirty();
  }

  public void SaveRaw()
  {
    Model.SaveRawModel();
    RaisePropertyChanged(nameof(IsDirty));
  }

  public void MarkDirty()
  {
    Model.MarkDirty();
    RaisePropertyChanged(nameof(IsDirty));
  }

  public void SaveIfDirty(bool rebuild)
  {
    if(IsDirty)
    {
      if(rebuild)
      {
        RebuildModel();
      }
      SaveRaw();
    }
  }

  public bool IsDirty => Model.IsDirty;

  public IEnumerable<IconLoadJob> GetIconLoadJobs(bool reload)
  {
    foreach(var host in Tiles)
    {
      if(host.Tile != null)
      {
        foreach(var job in host.Tile.GetIconLoadJobs(reload))
        {
          yield return job;
        }
      }
    }
  }

  public IconLoadQueue IconLoadQueue { get => Shelf.Rack.IconLoadQueue; }

  /// <summary>
  /// Add empty tiles until the following properties hold:
  /// The number of tiles is a multiple of 4. And there is
  /// at least one row of tiles. If any changes were made,
  /// the model is rebuilt.
  /// </summary>
  /// <returns>
  /// True if the list was modified, false if it was already
  /// meeting the requirements. 
  /// </returns>
  public bool PadRow()
  {
    var rows = (Tiles.Count + 3) / 4;
    if(rows < 1)
    {
      rows = 1;
    }
    var expectedTileCount = rows * 4;
    var dirty = false;
    while(Tiles.Count < expectedTileCount)
    {
      dirty = true;
      var host = new TileHostViewModel(this);
      host.Tile = new EmptyTileViewModel(this, null);
      Tiles.Add(host);
    }
    if(dirty)
    {
      RebuildModel();
    }
    return dirty;
  }
}
