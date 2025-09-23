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

using LcLauncher.DataModel.Entities;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class ColumnViewModel: ViewModelBase
{
  public ColumnViewModel(
    RackViewModel rack,
    int columnIndex)
  {
    Rack = rack;
    Model = rack.Model.Columns[columnIndex];
    ColumnIndex = columnIndex;
    Shelves = new ObservableCollection<ShelfViewModel>();
    foreach(var shelf in Model.Shelves)
    {
      Shelves.Add(new ShelfViewModel(Rack, shelf));
    }
    //MoveMarkedShelfHereCommand = new DelegateCommand(
    //  p => MoveMarkedShelfHere(),
    //  p => CanMoveMarkedShelfHere());
    //CreateNewShelfHereCommand = new DelegateCommand(
    //  p => CreateNewShelfHere(),
    //  p => Rack.KeyShelf == null && Rack.KeyTile == null);
  }

  ///// <summary>
  ///// Move the marked shelf to the tail of this column.
  ///// </summary>
  //public ICommand MoveMarkedShelfHereCommand { get; }

  //public ICommand CreateNewShelfHereCommand { get; }

  public RackViewModel Rack { get; }

  public int ColumnIndex { get; }

  internal ColumnModel Model { get; }

  public ObservableCollection<ShelfViewModel> Shelves { get; }

  //internal void MoveShelf(
  //  int indexSource,
  //  int indexDestination)
  //{
  //  if(indexSource == indexDestination)
  //  {
  //    return;
  //  }
  //  if(indexSource < 0 || indexSource >= Shelves.Count)
  //  {
  //    throw new ArgumentOutOfRangeException(nameof(indexSource));
  //  }
  //  if(indexDestination < 0 || indexDestination >= Shelves.Count)
  //  {
  //    // For same-column moves, the maximum destination index is
  //    // the last shelf in the column, not the empty slot after!
  //    // Hence the '>=' instead of '>'.
  //    throw new ArgumentOutOfRangeException(nameof(indexDestination));
  //  }
  //  // Move the viewmodel
  //  Shelves.Move(indexSource, indexDestination);
  //  // Move the model
  //  var modelSource = Model[indexSource];
  //  Model.RemoveAt(indexSource);
  //  Model.Insert(indexDestination, modelSource);
  //  // Mark the container as dirty
  //  MarkRackDirty();
  //}

  //internal void MoveShelf(
  //  int indexSource,
  //  ColumnViewModel columnDestination,
  //  int indexDestination)
  //{
  //  if(columnDestination == this)
  //  {
  //    MoveShelf(indexSource, indexDestination);
  //    return;
  //  }
  //  else
  //  {
  //    if(columnDestination.Rack != Rack)
  //    {
  //      throw new ArgumentException(
  //        "Cannot move shelves between different racks");
  //    }
  //    if(indexSource < 0 || indexSource >= Shelves.Count)
  //    {
  //      throw new ArgumentOutOfRangeException(nameof(indexSource));
  //    }
  //    if(indexDestination < 0 || indexDestination > columnDestination.Shelves.Count)
  //    {
  //      // For not-same-column moves, the maximum destination index is
  //      // the last shelf in the column plus 1
  //      // Hence the '>' instead of '>='.
  //      throw new ArgumentOutOfRangeException(nameof(indexDestination));
  //    }

  //    // Move the viewmodel
  //    var shelfVm = Shelves[indexSource];
  //    Shelves.RemoveAt(indexSource);
  //    columnDestination.Shelves.Insert(indexDestination, shelfVm);

  //    // Move the model
  //    var shelfModel = Model[indexSource];
  //    Model.RemoveAt(indexSource);
  //    columnDestination.Model.Insert(indexDestination, shelfModel);

  //    // Mark the container as dirty
  //    MarkRackDirty();
  //  }
  //}

  //public void MarkRackDirty()
  //{
  //  Rack.MarkDirty();
  //}

  //private bool CanMoveMarkedShelfHere()
  //{
  //  var keyShelf = Rack.KeyShelf!;
  //  if(keyShelf == null)
  //  {
  //    return false;
  //  }
  //  var columnLocation = Rack.GetColumnTail(this);
  //  var sourceLocation = Rack.GetShelfLocation(keyShelf);
  //  if(sourceLocation == null)
  //  {
  //    return false;
  //  }
  //  if(columnLocation.ColumnIndex == sourceLocation.Value.ColumnIndex)
  //  {
  //    // Same column, so we cannot move to the last shelf
  //    return sourceLocation.Value.ShelfIndex < columnLocation.ShelfIndex;
  //  }
  //  else
  //  {
  //    // Different column, so we also can move to any location
  //    return true;
  //  }
  //}

  //private void MoveMarkedShelfHere()
  //{
  //  if(CanMoveMarkedShelfHere())
  //  {
  //    var keyShelf = Rack.KeyShelf!;
  //    var sourceLocation = Rack.GetShelfLocation(keyShelf);
  //    var destinationLocation = Rack.GetColumnTail(this);
  //    if(sourceLocation != null)
  //    {
  //      Rack.MoveShelf(
  //        sourceLocation.Value,
  //        destinationLocation);
  //    }
  //  }
  //  Rack.KeyShelf = null;
  //}

  //private void CreateNewShelfHere()
  //{
  //  var sourceLocation = Rack.GetColumnTail(this);
  //  var _ = Rack.CreateNewShelf(sourceLocation, null);
  //  // Todo: open editor
  //}

}
