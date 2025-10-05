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
using System.Windows;

using LcLauncher.DataModel;
using LcLauncher.DataModel.ChangeTracking;
using LcLauncher.DataModel.Entities;
using LcLauncher.DataModel.Store;
using LcLauncher.IconTools;
using LcLauncher.Main.Rack.Tile;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

using Ttelcl.Persistence.API;

namespace LcLauncher.Main.Rack;

public class RackViewModel:
  ViewModelBase, ICanQueueIcons, IDirtyHost, IWrapsModel<RackModel, RackData>
{

  public RackViewModel(
    MainViewModel owner,
    RackModel model)
  {
    var store = model.Store;
    var iconBucket = store.IconBucket;
    IconQueue = new IconJobQueue();
    IconLoader = new IconLoader(iconBucket);
    Owner = owner;
    Model = model;
    Columns = [];
    foreach(var columnModel in model.Columns)
    {
      var columnVm = new ColumnViewModel(this, columnModel);
      Columns.Add(columnVm);
    }
    FixColumnIndexes();
    //Model.TraceClaimStatus();
    Trace.TraceInformation(
      $"Constructing rack VM {Model.RackKey} with {Columns.Count} columns");
  }

  public MainViewModel Owner { get; }

  public RackModel Model { get; }

  public LauncherRackStore Store => Model.Store;

  public IconJobQueue IconQueue { get; }

  public IconLoader IconLoader { get; }

  public string Name => Model.RackName;

  public void Unload()
  {
    Trace.TraceWarning(
      $"Unloading rack '{Name}' (and saving what needs saving)");
    SaveDeep();
  }

  public ObservableCollection<ColumnViewModel> Columns { get; }

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
    var tileLists = new Dictionary<TickId, TileListViewModel>();
    GatherTileLists(tileLists);
    return tileLists.Values.ToList();
  }

  private void GatherTileLists(
    Dictionary<TickId, TileListViewModel> tileLists)
  {
    foreach(var shelf in AllShelves())
    {
      shelf.GatherTileLists(tileLists);
    }
  }

  ///// <summary>
  ///// Get the current location of a shelf in this rack,
  ///// or null if it is not in this rack.
  ///// </summary>
  //public ShelfLocation? GetShelfLocation(ShelfViewModel shelf)
  //{
  //  for(int i = 0; i < Columns.Count; i++)
  //  {
  //    var column = Columns[i];
  //    for(int j = 0; j < column.Shelves.Count; j++)
  //    {
  //      if(column.Shelves[j] == shelf)
  //      {
  //        return new ShelfLocation(i, j);
  //      }
  //    }
  //  }
  //  return null;
  //}

  //public ShelfLocation GetColumnTail(int columnIndex)
  //{
  //  if(columnIndex < 0 || columnIndex >= Columns.Count)
  //  {
  //    throw new ArgumentOutOfRangeException(nameof(columnIndex));
  //  }
  //  var column = Columns[columnIndex];
  //  return new ShelfLocation(columnIndex, column.Shelves.Count);
  //}

  //public ShelfLocation GetColumnTail(ColumnViewModel column)
  //{
  //  if(!Columns.Contains(column))
  //  {
  //    throw new ArgumentException(
  //      "Column is not part of this rack", nameof(column));
  //  }
  //  return GetColumnTail(column.ColumnIndex);
  //}

  //public void MoveShelf(ShelfLocation source, ShelfLocation destination)
  //{
  //  if(source.ColumnIndex < 0 || source.ColumnIndex >= Columns.Count)
  //  {
  //    throw new ArgumentOutOfRangeException(nameof(source));
  //  }
  //  if(destination.ColumnIndex < 0 || destination.ColumnIndex >= Columns.Count)
  //  {
  //    throw new ArgumentOutOfRangeException(nameof(destination));
  //  }
  //  var columnSource = Columns[source.ColumnIndex];
  //  var columnTarget = Columns[destination.ColumnIndex];
  //  // Check that the source is an existing shelf
  //  if(source.ShelfIndex < 0 
  //    || source.ShelfIndex >= columnSource.Shelves.Count)
  //  {
  //    throw new ArgumentOutOfRangeException(nameof(source));
  //  }
  //  var maxDestinationIndex = columnTarget.Shelves.Count-1;
  //  if(source.ColumnIndex != destination.ColumnIndex)
  //  {
  //    // For different-column moves, the maximum destination index
  //    // is the empty slot after the last shelf in the column.
  //    maxDestinationIndex++;
  //  }
  //  // Check that the destination is an existing shelf or the tail
  //  // of the column.
  //  if(destination.ShelfIndex < 0
  //    || destination.ShelfIndex > maxDestinationIndex)
  //  {
  //    throw new ArgumentOutOfRangeException(nameof(destination));
  //  }
  //  if(source.ColumnIndex == destination.ColumnIndex
  //    && source.ShelfIndex == destination.ShelfIndex)
  //  {
  //    MessageBox.Show(
  //      "Source and destination shelf are the same");
  //  }
  //  columnSource.MoveShelf(
  //    source.ShelfIndex, columnTarget, destination.ShelfIndex);
  //}

  //public ShelfViewModel? GetShelfByLocation(ShelfLocation location)
  //{
  //  if(location.ColumnIndex < 0 || location.ColumnIndex >= Columns.Count)
  //  {
  //    return null;
  //  }
  //  var column = Columns[location.ColumnIndex];
  //  if(location.ShelfIndex < 0 || location.ShelfIndex >= column.Shelves.Count)
  //  {
  //    return null;
  //  }
  //  return column.Shelves[location.ShelfIndex];
  //}

  //public void SaveShelvesIfModified()
  //{
  //  Trace.TraceInformation(
  //    $"Saving modified shelves in rack '{Name}' (if any)");
  //  foreach(var shelf in AllShelves())
  //  {
  //    shelf.SaveIfDirty();
  //  }
  //}

  //public void SaveDirtyTileLists()
  //{
  //  Trace.TraceInformation(
  //    $"Saving modified tile lists in rack '{Name}' (if any)");
  //  foreach(var tileList in GatherTileLists())
  //  {
  //    tileList.SaveIfDirty();
  //  }
  //}

  //public bool IsDirty { get => Model.IsDirty; }

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


  public TileHostViewModel? ClickTile {
    get => _clickTile;
    set {
      if(SetNullableInstanceProperty(ref _clickTile, value))
      {
      }
    }
  }
  private TileHostViewModel? _clickTile;

  public bool HasMarkedItems {
    get => KeyTile != null
      || KeyShelf != null;
  }

  public void QueueIcons(bool regenerate)
  {
    foreach(var column in Columns)
    {
      column.QueueIcons(regenerate);
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

  /// <summary>
  /// Implements <see cref="IDirtyHost.Save(bool)"/>.
  /// This only saves the rack entity itself (including its columns),
  /// but not its shelves and tile lists.
  /// </summary>
  /// <param name="ifDirty"></param>
  public void Save(bool ifDirty = true)
  {
    if(IsDirty || !ifDirty)
    {
      Model.RebuildEntity();
      Trace.TraceInformation(
        $"Saving rack '{Model.RackName}' ({Model.Id})");
      Store.PutRack(Model.Entity);
      IsDirty = false;
    }
  }

  public void SaveDeep(bool ifDirty = true)
  {
    Save(ifDirty);
    SaveDirtyShelves(ifDirty);
    SaveDirtyTileLists(ifDirty);
  }

  public void SaveDirtyShelves(bool ifDirty = true)
  {
    foreach(var shelf in AllShelves())
    {
      shelf.Save(ifDirty);
    }
  }

  public void SaveDirtyTileLists(bool ifDirty = true)
  {
    var tileLists = GatherTileLists();
    foreach(var tileList in tileLists)
    {
      tileList.Save(ifDirty);
    }
  }

  public void MarkAsDirty()
  {
    if(!IsDirty)
    {
      IsDirty = true;
      RaisePropertyChanged(nameof(IsDirty));
    }
  }

  public ShelfLocation GetColumnTail(ColumnViewModel column)
  {
    if(!Columns.Contains(column))
    {
      throw new ArgumentException(
        "Column is not part of this rack", nameof(column));
    }
    return new ShelfLocation(column, column.Shelves.Count);
  }

  /// <summary>
  /// Get the current location of a shelf in this rack,
  /// or null if it is not in this rack.
  /// </summary>
  public ShelfLocation? GetShelfLocation(ShelfViewModel shelf)
  {
    foreach(var column in Columns)
    {
      for(int j = 0; j < column.Shelves.Count; j++)
      {
        if(column.Shelves[j] == shelf)
        {
          return new ShelfLocation(column, j);
        }
      }
    }
    return null;
  }

  public void MoveShelf(ShelfLocation source, ShelfLocation destination)
  {
    if(!Columns.Contains(source.Column))
    {
      throw new ArgumentException(
        "Source column is not part of this rack", nameof(source));
    }
    if(!Columns.Contains(destination.Column))
    {
      throw new ArgumentException(
        "Source column is not part of this rack", nameof(destination));
    }
    var columnSource = source.Column;
    var columnTarget = destination.Column;
    // Check that the source is an existing shelf
    if(source.ShelfIndex < 0
      || source.ShelfIndex >= columnSource.Shelves.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(source));
    }
    var maxDestinationIndex = columnTarget.Shelves.Count-1;
    if(!Object.ReferenceEquals(columnSource, columnTarget))
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
    if(Object.ReferenceEquals(columnSource, columnTarget)
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
    var column = location.Column;
    if(!Columns.Contains(column))
    {
      throw new ArgumentException(
        "Column is not part of this rack", nameof(column));
    }
    if(location.ShelfIndex < 0 || location.ShelfIndex >= column.Shelves.Count)
    {
      return null;
    }
    return column.Shelves[location.ShelfIndex];
  }

  /// <summary>
  /// Tell the columns what index they are at
  /// </summary>
  public void FixColumnIndexes()
  {
    for(var i=0; i<Columns.Count; i++)
    {
      var column = Columns[i];
      column.ColumnIndex = i;
    }
  }

  internal bool MoveColumn(ColumnViewModel column, bool right)
  {
    var index = Columns.IndexOf(column);
    if(index < 0)
    {
      Trace.TraceError(
        "Attempt to move a column that isn't in this rack");
      return false;
    }
    var newIndex = index + (right ? 1 : -1);
    if(newIndex < 0 || newIndex >= Columns.Count)
    {
      Trace.TraceWarning(
        "Ignoring attempt to move a column before or after the rack");
      return false;
    }
    Columns.Move(index, newIndex);
    var columnModel = Model.Columns[index];
    Model.Columns.RemoveAt(index);
    Model.Columns.Insert(newIndex, columnModel);
    FixColumnIndexes();
    MarkAsDirty();
    Save();
    return true;
  }

  internal ShelfViewModel CreateNewShelf(
    ShelfLocation location,
    string? title = null,
    string? initialTheme = null)
  {
    initialTheme ??= Owner.DefaultTheme;
    var columnVm = location.Column;
    if(location.ShelfIndex < 0
      || location.ShelfIndex > columnVm.Shelves.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(location));
    }
    var shelfId = TickId.New();
    var shelfData = new ShelfData(
      title ?? $"Unnamed shelf {shelfId}",
      false,
      initialTheme,
      shelfId);
    var shelfModel = new ShelfModel(
      columnVm.Model,
      shelfData);
    var shelfVm = new ShelfViewModel(this, shelfModel);
    // Insert VM into the column
    columnVm.Shelves.Insert(
      location.ShelfIndex, shelfVm);
    // Insert model into the column
    columnVm.Model.Shelves.Insert(
      location.ShelfIndex, shelfModel);
    shelfVm.MarkAsDirty();
    shelfVm.Save();
    MarkAsDirty();
    Save();
    return shelfVm;
  }

}
