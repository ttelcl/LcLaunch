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

using LcLauncher.DataModel;
//using LcLauncher.IconUpdates;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

using LcLauncher.DataModel.Entities;
using Ttelcl.Persistence.API;
using LcLauncher.IconTools;

namespace LcLauncher.Main.Rack.Tile;

public class TileListViewModel:
  ViewModelBase, ICanQueueIcons /*, IPersisted*/
{

  public TileListViewModel(
    ShelfViewModel shelf,
    TileListModel model)
  {
    Shelf = shelf;
    Model = model;
    TileListId = model.Id;
    Tiles = new ObservableCollection<TileHostViewModel>();
    foreach(var tile in model.Entity.Tiles)
    {
      var host = new TileHostViewModel(this);
      var tileVm = TileViewModel.Create(this, tile);
      host.Tile = tileVm;
      Tiles.Add(host);
    }
    //AddEmptyRowCommand = new DelegateCommand(
    //  p => AddEmptyRow(),
    //  p => CanAddEmptyRow());
    //RemoveLastEmptyRowCommand = new DelegateCommand(
    //  p => RemoveLastEmptyRow(),
    //  p => CanRemoveLastEmptyRow());
    //PadRow();
  }

  //public ICommand AddEmptyRowCommand { get; }

  //public ICommand RemoveLastEmptyRowCommand { get; }

  public ShelfViewModel Shelf { get; }

  public RackViewModel Rack => Shelf.Rack;

  public TileListModel Model { get; }

  public TickId TileListId {  get; }

  public bool IsPrimary { get => Model.Id == Shelf.Model.Id; }

  public ObservableCollection<TileHostViewModel> Tiles { get; }

  public bool ContainsKeyTile()
  {
    var keyTile = Rack.KeyTile;
    return keyTile!=null && Tiles.Contains(keyTile);
  }

  //public TileListViewModel CreateClone()
  //{
  //  SaveIfDirty(); // make sure the model is up to date
  //  var cloneModel = Model.CreateClone();
  //  var clone = new TileListViewModel(
  //    Rack.IconLoadQueue,
  //    Shelf,
  //    cloneModel);
  //  return clone;
  //}

  ///// <summary>
  ///// Rebuild the persisted model from the viewmodels
  ///// </summary>
  //public void RebuildModel()
  //{
  //  var newTiles = new List<TileData?>();
  //  foreach(var tile in Tiles)
  //  {
  //    newTiles.Add(tile?.Tile?.GetModel());
  //  }
  //  Model.RawTiles.Clear();
  //  Model.RawTiles.AddRange(newTiles);
  //  Model.MarkDirty();
  //}

  ///// <summary>
  ///// Save, without rebuilding the model (assuming it is already)
  ///// </summary>
  //private void SaveRaw()
  //{
  //  Trace.TraceInformation(
  //    $"Saving tile list {Model.Id}");
  //  Model.SaveRawModel();
  //  RaisePropertyChanged(nameof(IsDirty));
  //}

  ///// <inheritdoc/>
  //public void MarkDirty()
  //{
  //  Model.MarkDirty();
  //  RaisePropertyChanged(nameof(IsDirty));
  //}

  ///// <inheritdoc/>
  //public void SaveIfDirty()
  //{
  //  if(IsDirty)
  //  {
  //    RebuildModel();
  //    SaveRaw();
  //  }
  //}

  ///// <inheritdoc/>
  //public bool IsDirty => Model.IsDirty;

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
      Tiles[^4].IsEmpty &&
      Tiles.Count > 4;
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

  public void QueueIcons(bool regenerate)
  {
    foreach(var tile in Tiles)
    {
      tile.QueueIcons(regenerate);
    }
  }

  //public bool AddEmptyRow()
  //{
  //  if(CanAddEmptyRow())
  //  {
  //    for(var i = 0; i < 4; i++)
  //    {
  //      var host = new TileHostViewModel(this);
  //      host.Tile = new EmptyTileViewModel(this, Model2.TileData.EmptyTile());
  //      Tiles.Add(host);
  //    }
  //    MarkDirty();
  //    //SaveIfDirty(); // TODO: use autosave instead
  //    return true;
  //  }
  //  return false;
  //}

  //public bool RemoveLastEmptyRow()
  //{
  //  if(CanRemoveLastEmptyRow())
  //  {
  //    Tiles.RemoveAt(Tiles.Count - 1);
  //    Tiles.RemoveAt(Tiles.Count - 1);
  //    Tiles.RemoveAt(Tiles.Count - 1);
  //    Tiles.RemoveAt(Tiles.Count - 1);
  //    MarkDirty();
  //    //SaveIfDirty(); // TODO: use autosave instead
  //    return true;
  //  }
  //  return false;
  //}

  ///// <summary>
  ///// Add empty tiles until the following properties hold:
  ///// The number of tiles is a multiple of 4. And there is
  ///// at least one row of tiles. If any changes were made,
  ///// the model is rebuilt.
  ///// </summary>
  ///// <returns>
  ///// True if the list was modified, false if it was already
  ///// meeting the requirements. 
  ///// </returns>
  //public bool PadRow()
  //{
  //  var rows = (Tiles.Count + 3) / 4;
  //  if(rows < 1)
  //  {
  //    rows = 1;
  //  }
  //  var expectedTileCount = rows * 4;
  //  var dirty = false;
  //  while(Tiles.Count < expectedTileCount)
  //  {
  //    dirty = true;
  //    var host = new TileHostViewModel(this);
  //    host.Tile = new EmptyTileViewModel(this, Model2.TileData.EmptyTile());
  //    Tiles.Add(host);
  //  }
  //  if(dirty)
  //  {
  //    RebuildModel();
  //  }
  //  return dirty;
  //}

  //public void InsertEmptyTile(int position)
  //{
  //  if(position < 0)
  //  {
  //    throw new ArgumentOutOfRangeException(nameof(position));
  //  }
  //  if(position >= Tiles.Count)
  //  {
  //    // Interpret this request 'creatively': add an entire row and be done
  //    AddEmptyRow();
  //    return;
  //  }
  //  if(LastTileIsEmpty())
  //  {
  //    // Remove the last empty tile to make space
  //    Tiles.RemoveAt(Tiles.Count - 1);
  //  }
  //  else
  //  {
  //    AddEmptyRow();
  //    // Remove the last empty tile to make space
  //    Tiles.RemoveAt(Tiles.Count - 1);
  //  }
  //  var host = new TileHostViewModel(this);
  //  host.Tile = new EmptyTileViewModel(this, Model2.TileData.EmptyTile());
  //  Tiles.Insert(position, host);
  //  MarkDirty();
  //  //SaveIfDirty();
  //}

  //public void InsertEmptyTile(TileHostViewModel host)
  //{
  //  if(host.TileList != this)
  //  {
  //    throw new ArgumentException(
  //      "Host does not belong to this tile list");
  //  }
  //  var index = Tiles.IndexOf(host);
  //  InsertEmptyTile(index);
  //}

  //internal void GatherTileLists(Dictionary<Guid, TileListViewModel> buffer)
  //{
  //  if(!buffer.ContainsKey(TileListId))
  //  {
  //    buffer.Add(TileListId, this);
  //    foreach(var tile in Tiles)
  //    {
  //      if(tile.Tile != null && tile.Tile is GroupTileViewModel groupVm)
  //      {
  //        groupVm.ChildTiles.GatherTileLists(buffer);
  //      }
  //    }
  //  }
  //  else
  //  {
  //    Trace.TraceError(
  //      $"Encountered duplicate tile list reference {TileListId}");
  //  }
  //}
}
