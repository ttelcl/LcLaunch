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
public class TileListModel: IModel<TileListData>
{
  public TileListModel(
    ShelfModel shelf,
    TileListData tileListEntity)
  {
    Shelf = shelf;
    Entity = tileListEntity;
  }

  /// <summary>
  /// The shelf directly or indirectly owning this Tile List
  /// </summary>
  public ShelfModel Shelf { get; }

  public TileListData Entity { get; }

  public RackModel Rack => Shelf.Rack;

  public TickId Id => Entity.Id;

  public bool IsTopLevel => Entity.Id == Shelf.Id;

}
