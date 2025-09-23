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

using Ttelcl.Persistence.API;

namespace LcLauncher.Models;

public class ColumnModel
{
  internal ColumnModel(
    RackModel rack,
    ColumnData columnEntity)
  {
    Rack = rack;
    ColumnEntity = columnEntity;
    Shelves = new List<ShelfModel>();
    var store = Rack.Store;
    foreach(var shelfId in ColumnEntity.Shelves)
    {
      var shelfEntity = store.GetShelf(shelfId);
      if(shelfEntity == null)
      {
        Trace.TraceError($"Shelf {shelfId} is missing");
      }
      else
      {
        var shelf = new ShelfModel(this, shelfEntity);
        Shelves.Add(shelf);
      }
    }
  }

  public RackModel Rack { get; }

  public ColumnData ColumnEntity { get; }

  public TickId Id => ColumnEntity.Id;

  public List<ShelfModel> Shelves { get; }
}
