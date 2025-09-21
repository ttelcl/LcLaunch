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

/// <summary>
/// Description of RackModel3
/// </summary>
public class RackModel3
{
  /// <summary>
  /// Create a new RackModel3, using prepared components
  /// </summary>
  public RackModel3(
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
      var shelf1Tiles = new TileListData(
        shelf1Id, [
          TileData.EmptyTile(),
          TileData.EmptyTile(),
          TileData.EmptyTile(),
          TileData.EmptyTile(),
        ]);
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
    RackEntity = entity;
    foreach(var columnEntity in RackEntity.Columns)
    {
      var column = new ColumnModel3(this, columnEntity);
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

  public RackData RackEntity { get; }

  public List<ColumnModel3> Columns { get; }
}
