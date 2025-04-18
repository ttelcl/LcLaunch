/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.IconUpdates;
using LcLauncher.Models;
using LcLauncher.Persistence;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class RackViewModel: ViewModelBase, IIconLoadJobSource
{
  public RackViewModel(
    MainViewModel owner,
    RackModel model)
  {
    IconLoadQueue = new IconLoadQueue(q => owner.RackQueueActivating(q));
    Owner = owner;
    Model = model;
    ColumnLeft = new ColumnViewModel(this, Model.Columns[0]);
    ColumnMiddle = new ColumnViewModel(this, Model.Columns[1]);
    ColumnRight = new ColumnViewModel(this, Model.Columns[2]);
  }

  public MainViewModel Owner { get; }

  public RackModel Model { get; }

  public ILcLaunchStore Store => Model.Store;

  public string Name => Model.RackName;

  public ColumnViewModel ColumnLeft { get; }

  public ColumnViewModel ColumnMiddle { get; }

  public ColumnViewModel ColumnRight { get; }

  public IEnumerable<ShelfViewModel> AllShelves()
  {
    var columns = new List<ColumnViewModel>() {
      ColumnLeft,
      ColumnMiddle,
      ColumnRight
    };
    foreach(var columnVm in columns)
    {
      foreach(var shelfVm in columnVm.Shelves)
      {
        yield return shelfVm;
      }
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

}
