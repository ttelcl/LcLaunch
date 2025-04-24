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

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class ColumnViewModel: ViewModelBase
{
  public ColumnViewModel(
    RackViewModel rack,
    List<ShelfModel> model)
  {
    Rack = rack;
    Model = model;
    Shelves = new ObservableCollection<ShelfViewModel>();
    foreach(var shelf in model)
    {
      Shelves.Add(new ShelfViewModel(Rack, shelf));
    }
  }

  public RackViewModel Rack { get; }

  List<ShelfModel> Model { get; }

  public ObservableCollection<ShelfViewModel> Shelves { get; }

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
      throw new ArgumentOutOfRangeException(nameof(indexDestination));
    }
    Shelves.Move(indexSource, indexDestination);
    var modelSource = Model[indexSource];
    Model.RemoveAt(indexSource);
    Model.Insert(indexDestination, modelSource);
    MarkRackDirty();
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
    MessageBox.Show(
      "Moving shelves between columns is not yet supported.",
      "Not implemented",
      MessageBoxButton.OK,
      MessageBoxImage.Information);
  }

  public void MarkRackDirty()
  {
    Rack.MarkDirty();
  }
}
