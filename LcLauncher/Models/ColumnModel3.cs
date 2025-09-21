/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.DataModel.Entities;

namespace LcLauncher.Models;

public class ColumnModel3
{
  internal ColumnModel3(
    RackModel3 rack,
    ColumnData entity)
  {
    Rack = rack;
    Entity = entity;
    Shelves = new List<ShelfModel3>();
    var store = Rack.Store;
    foreach(var shelfId in Entity.Shelves)
    {
      var shelfEntity = store.GetShelf(shelfId);
      if(shelfEntity == null)
      {
        Trace.TraceError($"Shelf {shelfId} is missing");
      }
      else
      {
        var shelf = new ShelfModel3(this, shelfEntity);
        Shelves.Add(shelf);
      }
    }
  }

  public RackModel3 Rack { get; }

  public ColumnData Entity { get; }

  public List<ShelfModel3> Shelves { get; }
}
