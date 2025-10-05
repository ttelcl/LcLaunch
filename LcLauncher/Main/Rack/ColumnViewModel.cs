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
using System.Windows.Input;

using LcLauncher.DataModel.ChangeTracking;
using LcLauncher.DataModel.Entities;
using LcLauncher.IconTools;
using LcLauncher.Main.Rack.Editors;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class ColumnViewModel:
  ViewModelBase, ICanQueueIcons, IDirtyPart,
  IWrapsModel<ColumnModel, ColumnData>
{
  public ColumnViewModel(
    RackViewModel rack,
    ColumnModel columnModel)
  {
    Rack = rack;
    Model = columnModel;
    Shelves = new ObservableCollection<ShelfViewModel>();
    foreach(var shelf in Model.Shelves)
    {
      Shelves.Add(new ShelfViewModel(Rack, shelf));
    }
    CheckIsEmpty();
    MoveMarkedShelfHereCommand = new DelegateCommand(
      p => MoveMarkedShelfHere(),
      p => CanMoveMarkedShelfHere());
    CreateNewShelfHereCommand = new DelegateCommand(
      p => CreateNewShelfHere(),
      p => Rack.KeyShelf == null && Rack.KeyTile == null);
    MoveLeftCommand = new DelegateCommand(
      p => MoveLeft(),
      p => !IsFirst);
    MoveRightCommand = new DelegateCommand(
      p => MoveRight(),
      p => !IsLast);
    DeleteColumnCommand = new DelegateCommand(
      p => DeleteThisColumn(),
      p => CanDelete);
    AddNewColumnCommand = new DelegateCommand(
      p => AddNewColumn(),
      p => IsLast);
  }

  /// <summary>
  /// Move the marked shelf to the tail of this column.
  /// </summary>
  public ICommand MoveMarkedShelfHereCommand { get; }

  public ICommand CreateNewShelfHereCommand { get; }

  public ICommand MoveLeftCommand { get; }

  public ICommand MoveRightCommand { get; }

  public ICommand DeleteColumnCommand { get; }

  public ICommand AddNewColumnCommand { get; }

  public RackViewModel Rack { get; }

  public string Title {
    get => Model.ColumnTitle;
    set {
      var old = Model.ColumnTitle;
      if(ValueChanged(old, value))
      {
        Model.ColumnTitle = value;
      }
    }
  }

  internal ColumnModel Model { get; }

  public ObservableCollection<ShelfViewModel> Shelves { get; }

  public void QueueIcons(bool regenerate)
  {
    foreach(var shelf in Shelves)
    {
      shelf.QueueIcons(regenerate);
    }
  }

  public IDirtyHost DirtyHost => Rack;

  /// <summary>
  /// This column index. This is set by the rack when columns
  /// shuffle around (setting this does not affect the actual position)
  /// </summary>
  public int ColumnIndex {
    get => _columnIndex;
    set {
      if(SetValueProperty(ref _columnIndex, value))
      {
        // Separate function, since it sometimes needs running
        // manually:
        FixColumnIndex();
      }
    }
  }
  private int _columnIndex;

  internal void FixColumnIndex()
  {
    IsFirst = _columnIndex == 0;
    IsLast = _columnIndex == Rack.Columns.Count - 1;
    CheckCanDelete();
  }

  public bool IsFirst {
    get => _isFirst;
    private set {
      if(SetValueProperty(ref _isFirst, value))
      {
      }
    }
  }
  private bool _isFirst = true;

  public bool IsLast {
    get => _isLast;
    private set {
      if(SetValueProperty(ref _isLast, value))
      {
        CheckCanDelete();
      }
    }
  }
  private bool _isLast;

  public bool IsEmpty {
    get => _isEmpty;
    private set {
      if(SetValueProperty(ref _isEmpty, value))
      {
        CheckCanDelete();
      }
    }
  }
  private bool _isEmpty;

  internal void CheckIsEmpty()
  {
    IsEmpty = Shelves.Count == 0;
  }

  public bool CanDelete {
    get => _canDelete;
    private set {
      if(SetValueProperty(ref _canDelete, value))
      {
      }
    }
  }
  private bool _canDelete;

  internal void CheckCanDelete()
  {
    CanDelete = IsEmpty && Rack.Columns.Count > 1 && !Rack.HasMarkedItems;
  }

  public void MarkAsDirty()
  {
    DirtyHost.MarkAsDirty();
  }

  internal void MoveShelf(
    int indexSource,
    int indexDestination)
  {
    if(indexSource == indexDestination)
    {
      return;
    }
    if(indexSource < 0 || indexSource >= Shelves.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(indexSource));
    }
    if(indexDestination < 0 || indexDestination >= Shelves.Count)
    {
      // For same-column moves, the maximum destination index is
      // the last shelf in the column, not the empty slot after!
      // Hence the '>=' instead of '>'.
      throw new ArgumentOutOfRangeException(nameof(indexDestination));
    }
    // Move the viewmodel
    Shelves.Move(indexSource, indexDestination);
    // Move the model
    var modelShelves = Model.Shelves;
    var modelSource = modelShelves[indexSource];
    modelShelves.RemoveAt(indexSource);
    modelShelves.Insert(indexDestination, modelSource);
    // Mark the container as dirty
    MarkAsDirty();
  }

  internal void MoveShelf(
    int indexSource,
    ColumnViewModel columnDestination,
    int indexDestination)
  {
    if(columnDestination == this)
    {
      MoveShelf(indexSource, indexDestination);
      return;
    }
    else
    {
      if(columnDestination.Rack != Rack)
      {
        throw new ArgumentException(
          "Cannot move shelves between different racks");
      }
      if(indexSource < 0 || indexSource >= Shelves.Count)
      {
        throw new ArgumentOutOfRangeException(nameof(indexSource));
      }
      if(indexDestination < 0 || indexDestination > columnDestination.Shelves.Count)
      {
        // For not-same-column moves, the maximum destination index is
        // the last shelf in the column plus 1
        // Hence the '>' instead of '>='.
        throw new ArgumentOutOfRangeException(nameof(indexDestination));
      }

      // Move the viewmodel
      var shelfVm = Shelves[indexSource];
      Shelves.RemoveAt(indexSource);
      columnDestination.Shelves.Insert(indexDestination, shelfVm);

      // Move the model
      var shelfModel = Model.Shelves[indexSource];
      Model.Shelves.RemoveAt(indexSource);
      columnDestination.Model.Shelves.Insert(indexDestination, shelfModel);

      CheckIsEmpty();
      columnDestination.CheckIsEmpty();

      // Mark the container as dirty (logically you'd expect to mark both
      // source and destination column as dirty, but they both forward that
      // to the rack).
      Rack.MarkAsDirty();
    }
  }

  private bool CanMoveMarkedShelfHere()
  {
    var keyShelf = Rack.KeyShelf;
    if(keyShelf == null)
    {
      return false;
    }
    var columnLocation = Rack.GetColumnTail(this);
    var sourceLocation = Rack.GetShelfLocation(keyShelf);
    if(sourceLocation == null)
    {
      return false;
    }
    if(sourceLocation.Value.Column == this)
    {
      // Counting is different for the marked shelf's own column;
      // the net effect is that shelves cannot be moved to the end by
      // clicking in the empty column space.
      return false;
    }
    if(Object.ReferenceEquals(this, sourceLocation.Value.Column))
    {
      // Same column, so we cannot move to the last shelf
      return sourceLocation.Value.ShelfIndex < columnLocation.ShelfIndex;
    }
    else
    {
      // Different column, so we also can move to any location
      return true;
    }
  }

  private void MoveMarkedShelfHere()
  {
    if(CanMoveMarkedShelfHere())
    {
      var keyShelf = Rack.KeyShelf!;
      var sourceLocation = Rack.GetShelfLocation(keyShelf);
      var destinationLocation = Rack.GetColumnTail(this);
      if(sourceLocation != null)
      {
        Rack.MoveShelf(
          sourceLocation.Value,
          destinationLocation);
      }
    }
    Rack.KeyShelf = null;
  }

  private void CreateNewShelfHere()
  {
    // enforce preconditions
    Rack.KeyTile = null;
    Rack.KeyShelf = null;
    var sourceLocation = Rack.GetColumnTail(this);
    var shelf = Rack.CreateNewShelf(sourceLocation, null);
    ShelfEditViewModel.Show(shelf);
  }

  private void MoveLeft()
  {
    Rack.MoveColumn(this, false);
  }

  private void MoveRight()
  {
    Rack.MoveColumn(this, true);
  }

  private void DeleteThisColumn()
  {
    if(!IsEmpty)
    {
      MessageBox.Show(
        "Cannot delete a non-empty column",
        "Error");
      return;
    }
    if(Rack.Columns.Count <= 1)
    {
      MessageBox.Show(
        "Cannot delete the last remaining column",
        "Error");
      return;
    }
    if(Rack.HasMarkedItems)
    {
      MessageBox.Show(
        "Cannot columns while any shelf or tile is marked / selected",
        "Error");
      return;
    }
    Rack.DeleteColumn(this);
  }

  private void AddNewColumn()
  {
    if(IsLast)
    {
      Rack.AppendNewColumn();
      FixColumnIndex();
    }
  }
}
