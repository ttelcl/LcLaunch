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
using LcLauncher.DataModel.Store;

using Ttelcl.Persistence.API;

namespace LcLauncher.Models;

public class RackModel: IRebuildableModel<RackData>
{
  /// <summary>
  /// Create a new RackModel, using prepared components
  /// </summary>
  public RackModel(
    LauncherRackStore store,
    StoreKey rackKey)
  {
    if(rackKey.StoreTag != "rack")
    {
      throw new ArgumentException(
        "Unexpected store tag in the rack identifier");
    }
    RackKey = rackKey;
    Store = store;
    Columns = [];
    var entity = Store.FindRack();
    if(entity == null)
    {
      // The rack is brand new - the rack entity object wasn't created in
      // the persistence layer yet
      var shelf1Id = TickId.New();
      var shelf1 = new ShelfData(
        "Untitled Shelf", false, null, shelf1Id);
      var shelf1Tiles = TileListData.CreateNew(shelf1Id);
      Store.PutShelf(shelf1);
      Store.PutTiles(shelf1Tiles);
      entity = new RackData(
        TickId.New(),
        RackName,
        [
          new ColumnData(TickId.New(), [ shelf1Id, ], "Column 1"),
          new ColumnData(TickId.New(), [], "Column 2"),
          new ColumnData(TickId.New(), [], "Column 3"),
        ]);
      Store.PutRack(entity);
    }
    Entity = entity;
    foreach(var columnEntity in Entity.Columns)
    {
      var column = new ColumnModel(this, columnEntity);
      Columns.Add(column);
    }
  }

  /// <summary>
  /// The exact rack identifier (including name and backing store
  /// provider name)
  /// </summary>
  public StoreKey RackKey { get; }

  public string RackName => RackKey.StoreName;

  public LauncherRackStore Store { get; }

  public LauncherHyperStore HyperStore => Store.Owner;

  public RackData Entity { get; }

  public TickId Id => Entity.Id;

  public List<ColumnModel> Columns { get; }

  public void RebuildEntity()
  {
    // Rack ID is immutable
    // Rack name is immutable too (because it is used as the actual ID!)
    // That leaves the columns to be rebuilt
    var newColumnData = new List<ColumnData>();
    foreach(var column in Columns)
    {
      column.RebuildEntity();
      newColumnData.Add(column.Entity);
    }
    Entity.Columns.Clear();
    Entity.Columns.AddRange(newColumnData);
  }
}
