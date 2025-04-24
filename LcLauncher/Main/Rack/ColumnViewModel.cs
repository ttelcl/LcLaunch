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

namespace LcLauncher.Main.Rack;

public class ColumnViewModel: ViewModelBase
{
  public ColumnViewModel(
    RackViewModel rack,
    List<ShelfModel> column)
  {
    Rack = rack;
    Column = column;
    Shelves = new ObservableCollection<ShelfViewModel>();
    foreach(var shelf in column)
    {
      Shelves.Add(new ShelfViewModel(Rack, shelf));
    }
  }

  public RackViewModel Rack { get; }

  List<ShelfModel> Column { get; }

  public ObservableCollection<ShelfViewModel> Shelves { get; }

  public void MoveShelf(
    ShelfViewModel shelfSource,
    ShelfViewModel shelfTarget) 
  {
    var indexSource = Shelves.IndexOf(shelfSource);
    var indexTarget = Shelves.IndexOf(shelfTarget);
    if(indexSource < 0)
    {
      throw new ArgumentException(
        "Source shelf not found in column");
    }
    if(indexTarget < 0)
    {
      throw new ArgumentException(
        "Target shelf not found in column");
    }
    if(indexSource == indexTarget)
    {
      return;
    }
    Shelves.Move(indexSource, indexTarget);
    var modelSource = Column[indexSource];
    Column.RemoveAt(indexSource);
    Column.Insert(indexTarget, modelSource);
    MarkRackDirty();
  }

  public void MarkRackDirty()
  {
    Rack.MarkDirty();
  }
}
