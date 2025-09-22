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

public class ShelfModel3: IModel<ShelfData>
{
  internal ShelfModel3(
    ColumnModel3 column,
    ShelfData shelfEntity)
  {
    Column = column;
    Entity = shelfEntity;
    var tilesEntity = Store.GetTiles(Id);
    if(tilesEntity == null)
    {
      Trace.TraceWarning(
        $"Tiles for shelf {Id} are missing. Creating a default");
      tilesEntity = TileListData.CreateNew(Id);
      Store.PutTiles(tilesEntity);
    }
    PrimaryTiles = new TileListModel3(this, tilesEntity);
  }

  public LauncherRackStore Store => Rack.Store;

  public ColumnModel3 Column { get; }

  public RackModel3 Rack => Column.Rack;

  public ShelfData Entity { get; }

  public TickId Id => Entity.Id;

  public TileListModel3 PrimaryTiles { get; }
}
