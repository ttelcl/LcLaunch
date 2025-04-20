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
using System.Windows.Input;

using LcLauncher.IconUpdates;
using LcLauncher.Models;
using LcLauncher.Persistence;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

public class TileListViewModel: ViewModelBase, IIconLoadJobSource, IPersisted
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
    AddEmptyRowCommand = new DelegateCommand(
      p => AddEmptyRow(),
      p => CanAddEmptyRow());
    RemoveLastEmptyRowCommand = new DelegateCommand(
      p => RemoveLastEmptyRow(),
      p => CanRemoveLastEmptyRow());
    PadRow();
  }

  public ICommand AddEmptyRowCommand { get; }

  public ICommand RemoveLastEmptyRowCommand { get; }

  public ShelfViewModel Shelf { get; }

  public RackViewModel Rack => Shelf.Rack;

  public TileListModel Model { get; }

  public ILauncherIconCache IconCache => Model.IconCache;

  public bool IsPrimary { get => Model.Id == Shelf.Model.Id; }

  public ObservableCollection<TileHostViewModel> Tiles { get; }

  public IconListQueue IconJobQueue { get; }

  public TileListViewModel CreateClone()
  {
    SaveIfDirty(); // make sure the model is up to date
    var cloneModel = Model.CreateClone();
    var clone = new TileListViewModel(
      Rack.IconLoadQueue,
      Shelf,
      cloneModel);
    return clone;
  }

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

  /// <summary>
  /// Save, without rebuilding the model (assuming it is already)
  /// </summary>
  private void SaveRaw()
  {
    Trace.TraceInformation(
      $"Saving tile list {Model.Id}");
    Model.SaveRawModel();
    RaisePropertyChanged(nameof(IsDirty));
  }

  /// <inheritdoc/>
  public void MarkDirty()
  {
    Model.MarkDirty();
    RaisePropertyChanged(nameof(IsDirty));
  }

  /// <inheritdoc/>
  public void SaveIfDirty()
  {
    if(IsDirty)
    {
      RebuildModel();
      SaveRaw();
    }
  }

  /// <inheritdoc/>
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

  public IconLoadQueue IconLoadQueue { get => Rack.IconLoadQueue; }

  /// <summary>
  /// True if there is at least one tile and the last tile
  /// in the list is empty (i.e.: it could be removed to make space).
  /// </summary>
  public bool LastTileIsEmpty()
  {
    if(Tiles.Count == 0)
    {
      // No tiles, so no empty tile
      return false;
    }
    var lastTile = Tiles.Last();
    return lastTile.IsEmpty;
  }

  /// <summary>
  /// True if there is at least one full row.
  /// </summary>
  public bool HasRows()
  {
    return Tiles.Count >= 4;
  }

  /// <summary>
  /// True if the number of tiles is a multiple of 4 and
  /// at least 4: all rows are full and there is at least one row.
  /// </summary>
  public bool IsPadded()
  {
    return Tiles.Count % 4 == 0 && Tiles.Count >= 4;
  }

  public bool CanRemoveLastEmptyRow()
  {
    if(!IsPadded())
    {
      // We are in some intermediate state
      return false;
    }
    return
      Tiles[^1].IsEmpty &&
      Tiles[^2].IsEmpty &&
      Tiles[^3].IsEmpty &&
      Tiles[^4].IsEmpty;
  }

  public bool CanAddEmptyRow()
  {
    if(Tiles.Count % 4 != 0)
    {
      // We are in some intermediate state
      return false;
    }
    if(Tiles.Count == 0)
    {
      return true;
    }
    return // at least one of the last row tiles is in use
      !Tiles[^1].IsEmpty ||
      !Tiles[^2].IsEmpty ||
      !Tiles[^3].IsEmpty ||
      !Tiles[^4].IsEmpty;
  }

  public bool AddEmptyRow()
  {
    if(CanAddEmptyRow())
    {
      for(var i = 0; i < 4; i++)
      {
        var host = new TileHostViewModel(this);
        host.Tile = new EmptyTileViewModel(
          this, TileData.EmptyTile());
        Tiles.Add(host);
      }
      MarkDirty();
      SaveIfDirty(); // TODO: use autosave instead
      return true;
    }
    return false;
  }

  public bool RemoveLastEmptyRow()
  {
    if(CanRemoveLastEmptyRow())
    {
      Tiles.RemoveAt(Tiles.Count - 1);
      Tiles.RemoveAt(Tiles.Count - 1);
      Tiles.RemoveAt(Tiles.Count - 1);
      Tiles.RemoveAt(Tiles.Count - 1);
      MarkDirty();
      SaveIfDirty(); // TODO: use autosave instead
      return true;
    }
    return false;
  }

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

  public bool ContainsKeyTile()
  {
    var keyTile = Rack.KeyTile;
    return keyTile!=null && Tiles.Contains(keyTile);
  }
}
