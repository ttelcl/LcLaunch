﻿/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LcLauncher.IconUpdates;
using LcLauncher.Main.Rack.Tile;
using LcLauncher.Models;
using LcLauncher.Persistence;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class RackViewModel: ViewModelBase, IIconLoadJobSource, IPersisted
{
  private List<ColumnViewModel> _columns;

  public RackViewModel(
    MainViewModel owner,
    RackModel model)
  {
    IconLoadQueue = new IconLoadQueue(q => owner.RackQueueActivating(q));
    Owner = owner;
    Model = model;
    _columns = new List<ColumnViewModel>();
    Columns = _columns.AsReadOnly();
    ColumnLeft = new ColumnViewModel(this, 0);
    ColumnMiddle = new ColumnViewModel(this, 1);
    ColumnRight = new ColumnViewModel(this, 2);
    _columns.Add(ColumnLeft);
    _columns.Add(ColumnMiddle);
    _columns.Add(ColumnRight);
    Model.TraceClaimStatus();
  }

  public MainViewModel Owner { get; }

  public RackModel Model { get; }

  public ILcLaunchStore Store => Model.Store;

  public string Name => Model.RackName;

  public ColumnViewModel ColumnLeft { get; }

  public ColumnViewModel ColumnMiddle { get; }

  public ColumnViewModel ColumnRight { get; }

  public IReadOnlyList<ColumnViewModel> Columns { get; }

  public IEnumerable<ShelfViewModel> AllShelves()
  {
    foreach(var columnVm in Columns)
    {
      foreach(var shelfVm in columnVm.Shelves)
      {
        yield return shelfVm;
      }
    }
  }

  /// <summary>
  /// Gather all tile lists referenced in the rack by walking
  /// the shelves, tile lists and group tiles.
  /// </summary>
  public List<TileListViewModel> GatherTileLists()
  {
    var tileLists = new Dictionary<Guid, TileListViewModel>();
    GatherTileLists(tileLists);
    return tileLists.Values.ToList();
  }

  /// <summary>
  /// Get the current location of a shelf in this rack,
  /// or null if it is not in this rack.
  /// </summary>
  public ShelfLocation? GetShelfLocation(ShelfViewModel shelf)
  {
    for(int i = 0; i < Columns.Count; i++)
    {
      var column = Columns[i];
      for(int j = 0; j < column.Shelves.Count; j++)
      {
        if(column.Shelves[j] == shelf)
        {
          return new ShelfLocation(i, j);
        }
      }
    }
    return null;
  }

  public ShelfLocation GetColumnTail(int columnIndex)
  {
    if(columnIndex < 0 || columnIndex >= Columns.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(columnIndex));
    }
    var column = Columns[columnIndex];
    return new ShelfLocation(columnIndex, column.Shelves.Count);
  }

  public ShelfLocation GetColumnTail(ColumnViewModel column)
  {
    if(!Columns.Contains(column))
    {
      throw new ArgumentException(
        "Column is not part of this rack", nameof(column));
    }
    return GetColumnTail(column.ColumnIndex);
  }

  public void MoveShelf(ShelfLocation source, ShelfLocation destination)
  {
    if(source.ColumnIndex < 0 || source.ColumnIndex >= Columns.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(source));
    }
    if(destination.ColumnIndex < 0 || destination.ColumnIndex >= Columns.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(destination));
    }
    var columnSource = Columns[source.ColumnIndex];
    var columnTarget = Columns[destination.ColumnIndex];
    // Check that the source is an existing shelf
    if(source.ShelfIndex < 0 
      || source.ShelfIndex >= columnSource.Shelves.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(source));
    }
    var maxDestinationIndex = columnTarget.Shelves.Count-1;
    if(source.ColumnIndex != destination.ColumnIndex)
    {
      // For different-column moves, the maximum destination index
      // is the empty slot after the last shelf in the column.
      maxDestinationIndex++;
    }
    // Check that the destination is an existing shelf or the tail
    // of the column.
    if(destination.ShelfIndex < 0
      || destination.ShelfIndex > maxDestinationIndex)
    {
      throw new ArgumentOutOfRangeException(nameof(destination));
    }
    if(source.ColumnIndex == destination.ColumnIndex
      && source.ShelfIndex == destination.ShelfIndex)
    {
      MessageBox.Show(
        "Source and destination shelf are the same");
    }
    columnSource.MoveShelf(
      source.ShelfIndex, columnTarget, destination.ShelfIndex);
  }

  public ShelfViewModel? GetShelfByLocation(ShelfLocation location)
  {
    if(location.ColumnIndex < 0 || location.ColumnIndex >= Columns.Count)
    {
      return null;
    }
    var column = Columns[location.ColumnIndex];
    if(location.ShelfIndex < 0 || location.ShelfIndex >= column.Shelves.Count)
    {
      return null;
    }
    return column.Shelves[location.ShelfIndex];
  }

  private void GatherTileLists(
    Dictionary<Guid, TileListViewModel> tileLists)
  {
    foreach(var shelf in AllShelves())
    {
      shelf.GatherTileLists(tileLists);
    }
  }

  public IconLoadQueue IconLoadQueue { get; }

  public IEnumerable<IconLoadJob> GetIconLoadJobs(bool reload)
  {
    foreach(var shelf in AllShelves())
    {
      foreach(var job in shelf.GetIconLoadJobs(reload))
      {
        yield return job;
      }
    }
  }

  public void SaveShelvesIfModified()
  {
    Trace.TraceInformation(
      $"Saving modified shelves in rack '{Name}' (if any)");
    foreach(var shelf in AllShelves())
    {
      shelf.SaveIfDirty();
    }
  }

  public void SaveDirtyTileLists()
  {
    Trace.TraceInformation(
      $"Saving modified tile lists in rack '{Name}' (if any)");
    foreach(var tileList in GatherTileLists())
    {
      tileList.SaveIfDirty();
    }
  }

  public bool IsDirty { get => Model.IsDirty; }

  /// <summary>
  /// The one shelf that is treated special. In UI feedback
  /// it may be referenced as 'cut' (like in Excel, where 'cutting'
  /// doesn't actually cut anything until it is used elsehwere).
  /// </summary>
  public ShelfViewModel? KeyShelf {
    // Note: interlinked with ShelfViewModel.IsKeyShelf
    get => _keyShelf;
    set {
      var oldShelf = _keyShelf;
      if(SetNullableInstanceProperty(ref _keyShelf, value))
      {
        if(oldShelf != null)
        {
          oldShelf.IsKeyShelf = false;
        }
        if(_keyShelf != null)
        {
          KeyTile = null; // KeyShelf and KeyTile are mutually exclusive
          _keyShelf.IsKeyShelf = true;
          Trace.TraceInformation(
            $"Key shelf changed to '{_keyShelf.ShelfId}'");
        }
        else
        {
          Trace.TraceInformation(
            $"Key shelf cleared");
        }
        RaisePropertyChanged(nameof(HasMarkedItems));
      }
    }
  }
  private ShelfViewModel? _keyShelf;

  public TileHostViewModel? KeyTile {
    // Note: interlinked with TileHostViewModel.IsKeyTile
    get => _keyTile;
    set {
      var oldTile = _keyTile;
      if(SetNullableInstanceProperty(ref _keyTile, value))
      {
        if(oldTile != null)
        {
          oldTile.IsKeyTile = false;
        }
        if(_keyTile != null)
        {
          KeyShelf = null; // KeyShelf and KeyTile are mutually exclusive
          _keyTile.IsKeyTile = true;
          Trace.TraceInformation(
            $"Key tile changed to '{_keyTile}'");
        }
        else
        {
          Trace.TraceInformation(
            $"Key tile cleared");
        }
        RaisePropertyChanged(nameof(HasMarkedItems));
      }
    }
  }
  private TileHostViewModel? _keyTile;

  public bool HasMarkedItems {
    get => KeyTile != null
      || KeyShelf != null;
  }

  public void MarkDirty()
  {
    Model.MarkDirty();
    RaisePropertyChanged(nameof(IsDirty));
  }

  public void SaveIfDirty()
  {
    if(IsDirty)
    {
      Trace.TraceInformation(
        $"Saving rack metadata '{Name}'");
      Model.RebuildRackData();
      Model.Save();
      RaisePropertyChanged(nameof(IsDirty));
    }
  }

  internal ShelfViewModel CreateNewShelf(
    ShelfLocation location,
    string? title = null,
    string? initialTheme = null)
  {
    initialTheme ??= Owner.DefaultTheme;
    var columnVm = Columns[location.ColumnIndex];
    if(location.ShelfIndex < 0
      || location.ShelfIndex > columnVm.Shelves.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(location));
    }
    var shelfGuid = Guid.NewGuid();
    var shelfData = new ShelfData(
      title ?? $"Unnamed shelf {shelfGuid}",
      false,
      initialTheme);
    var shelfModel = new ShelfModel(
      Model,
      shelfGuid,
      shelfData);
    var shelfVm = new ShelfViewModel(this, shelfModel);
    // Insert VM into the column
    columnVm.Shelves.Insert(
      location.ShelfIndex, shelfVm);
    // Insert model into the column
    columnVm.Model.Insert(
      location.ShelfIndex, shelfModel);
    shelfVm.MarkDirty();
    shelfVm.SaveIfDirty();
    MarkDirty();
    SaveIfDirty();
    return shelfVm;
  }
}
