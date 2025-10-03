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

using Ttelcl.Persistence.API;

using LcLauncher.DataModel;
using LcLauncher.DataModel.ChangeTracking;
using LcLauncher.DataModel.Entities;
using LcLauncher.IconTools;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;
using LcLauncher.DataModel.Store;

namespace LcLauncher.Main.Rack.Tile;

public class TileListViewModel:
  ViewModelBase, ICanQueueIcons, IDirtyHost,
  IRebuildableWrapModel<TileListModel, TileListData>
{

  public TileListViewModel(
    ShelfViewModel shelf,
    TileListModel model,
    ITileListOwner owner)
  {
    Shelf = shelf;
    Model = model;
    Owner = owner;
    TileListId = model.Id;
    Tiles = new ObservableCollection<TileHostViewModel>();
    foreach(var tile in model.Entity.Tiles)
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

  public ITileListOwner Owner { get; }

  public LauncherRackStore Store => Shelf.Store;

  public TileListModel Model { get; }

  public TickId TileListId {  get; }

  public bool IsPrimary { get => Model.Id == Shelf.Model.Id; }

  public ObservableCollection<TileHostViewModel> Tiles { get; }

  public bool ContainsKeyTile()
  {
    var keyTile = Rack.KeyTile;
    return keyTile!=null && Tiles.Contains(keyTile);
  }

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

  /// <inheritdoc/>
  public void QueueIcons(bool regenerate)
  {
    foreach(var tile in Tiles)
    {
      tile.QueueIcons(regenerate);
    }
  }

  /// <inheritdoc/>
  public bool IsDirty {
    get => _isDirty;
    private set {
      if(SetValueProperty(ref _isDirty, value))
      {
      }
    }
  }
  private bool _isDirty;

  /// <inheritdoc/>
  public void Save(bool ifDirty = true)
  {
    if(IsDirty || !ifDirty)
    {
      RebuildEntity();
      Trace.TraceInformation(
        $"Saving tile list {Model.Id}");
      Store.PutTiles(Model.Entity);
      IsDirty = false;
    }
  }

  /// <inheritdoc/>
  public void MarkAsDirty()
  {
    IsDirty = true;
    Owner.OwnedTilesEdited();
  }

  /// <inheritdoc/>
  public void RebuildEntity()
  {
    var model = Model;
    var entity = model.Entity;
    var newTiles = new List<TileData?>();
    foreach(var tile in Tiles)
    {
      // GetModel() is exposed in the VM layer (although it could be reimplemented
      // in the model layer).
      // GetModel may return the existing model or create a new one. In this case
      // either is fine.
      newTiles.Add(tile.Tile?.GetModel());
    }
    entity.Tiles.Clear();
    entity.Tiles.AddRange(newTiles);
    MarkAsDirty();
  }

  public bool AddEmptyRow()
  {
    if(CanAddEmptyRow())
    {
      for(var i = 0; i < 4; i++)
      {
        var host = new TileHostViewModel(this);
        host.Tile = new EmptyTileViewModel(this, TileData.EmptyTile());
        Tiles.Add(host);
      }
      MarkAsDirty();
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
      MarkAsDirty();
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
      host.Tile = new EmptyTileViewModel(this, TileData.EmptyTile());
      Tiles.Add(host);
    }
    if(dirty)
    {
      // MarkAsDirty(); // included in RebuildEntity()
      RebuildEntity();
    }
    return dirty;
  }

  public void InsertEmptyTile(int position)
  {
    if(position < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(position));
    }
    if(position >= Tiles.Count)
    {
      // Interpret this request 'creatively': add an entire row and be done
      AddEmptyRow();
      return;
    }
    if(LastTileIsEmpty())
    {
      // Remove the last empty tile to make space
      Tiles.RemoveAt(Tiles.Count - 1);
    }
    else
    {
      AddEmptyRow();
      // Remove the last empty tile to make space
      Tiles.RemoveAt(Tiles.Count - 1);
    }
    var host = new TileHostViewModel(this);
    host.Tile = new EmptyTileViewModel(this, TileData.EmptyTile());
    Tiles.Insert(position, host);
    MarkAsDirty();
    //SaveIfDirty();
  }

  public void InsertEmptyTile(TileHostViewModel host)
  {
    if(host.TileList != this)
    {
      throw new ArgumentException(
        "Host does not belong to this tile list");
    }
    var index = Tiles.IndexOf(host);
    InsertEmptyTile(index);
  }

  internal void GatherTileLists(Dictionary<TickId, TileListViewModel> buffer)
  {
    if(!buffer.ContainsKey(TileListId))
    {
      buffer.Add(TileListId, this);
      foreach(var tile in Tiles)
      {
        if(tile.Tile != null && tile.Tile is GroupTileViewModel groupVm)
        {
          groupVm.ChildTiles.GatherTileLists(buffer);
        }
      }
    }
    else
    {
      Trace.TraceError(
        $"Encountered duplicate tile list reference {TileListId}");
    }
  }
}
