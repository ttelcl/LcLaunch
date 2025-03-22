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

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main;

public class ShelfContentViewModel: ViewModelBase
{
  public ShelfContentViewModel(
    ShelfViewModel owner,
    bool isPrimary)
  {
    Owner = owner;
    IsPrimary = isPrimary;
    Slots = new ObservableCollection<TileSlotViewModel>();
    RowCount = 1;
  }

  public ShelfViewModel Owner { get; }

  public bool IsPrimary { get; }

  public ObservableCollection<TileSlotViewModel> Slots {
    get;
  }

  public int RowCount {
    get => (Slots.Count+3) / 4;
    set {
      TrySetRowCount(value);
    }
  }

  private bool TrySetRowCount(int rowCount)
  {
    var result = true;
    if(rowCount < 1)
    {
      rowCount = 1; // minimum of 1 row
      result = false;
    }
    var oldCount = RowCount;
    if(rowCount == oldCount)
    {
      return result;
    }
    if(rowCount > oldCount)
    {
      var targetTileCount = rowCount * 4;
      while(Slots.Count < targetTileCount)
      {
        // Add a new empty slot
        Slots.Add(new TileSlotViewModel(this, Slots.Count));
      }
      RaisePropertyChanged(nameof(RowCount));
      return result;
    }
    // Remove slots row by row, failing the row removal if any slot is not empty
    for(var row = oldCount; row > rowCount; row--)
    {
      var rowStart = row * 4;
      for(var i = 0; i < 4; i++)
      {
        var slot = Slots[rowStart + i];
        if(!slot.IsEmpty)
        {
          // stop removing more rows
          if(RowCount != oldCount)
          {
            RaisePropertyChanged(nameof(RowCount));
          }
          return false;
        }
      }
      for(var i = 0; i < 4; i++)
      {
        Slots.RemoveAt(rowStart);
      }
    }
    return result;
  }
}
