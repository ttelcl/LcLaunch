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

public class ColumnModel: IRebuildableModel<ColumnData>
{
  internal ColumnModel(
    RackModel rack,
    ColumnData columnEntity)
  {
    Rack = rack;
    Entity = columnEntity;
    Shelves = new List<ShelfModel>();
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
        var shelf = new ShelfModel(this, shelfEntity);
        Shelves.Add(shelf);
      }
    }
  }

  public RackModel Rack { get; }

  public ColumnData Entity { get; }

  public string ColumnTitle {
    get => Entity.Title;
    set => Entity.Title = value;
  }

  public TickId Id => Entity.Id;

  public List<ShelfModel> Shelves { get; }

  public void RebuildEntity()
  {
    // Entity.Id is immutable
    // Entity.Title is already 'hot'
    // Rebuild shelf id list
    Entity.Shelves.Clear();
    Entity.Shelves.AddRange(Shelves.Select(shelfModel => shelfModel.Id));
  }
}
