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
public class TileListModel3: IModel<TileListData>
{
  public TileListModel3(
    ShelfModel3 shelf,
    TileListData tileListEntity)
  {
    Shelf = shelf;
    Entity = tileListEntity;
  }

  /// <summary>
  /// The shelf directly or indirectly owning this Tile List
  /// </summary>
  public ShelfModel3 Shelf { get; }

  public TileListData Entity { get; }

  public RackModel3 Rack => Shelf.Rack;

  public TickId Id => Entity.Id;

  public bool IsTopLevel => Entity.Id == Shelf.Id;

}
