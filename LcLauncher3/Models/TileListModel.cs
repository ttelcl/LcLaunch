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

/// <summary>
/// A tile list within a rack. Logically each tile list belongs
/// to a rack (not a shelf!)
/// </summary>
public class TileListModel: IModel<TileListData>
{
  public TileListModel(
    RackModel rack,
    TileListData tileListEntity)
  {
    Entity = tileListEntity;
    Rack = rack;
  }

  ///// <summary>
  ///// The shelf directly or indirectly owning this Tile List
  ///// </summary>
  //public ShelfModel Shelf { get; }

  public TileListData Entity { get; }

  public RackModel Rack { get; }

  public TickId Id => Entity.Id;

  //public bool IsTopLevel => Entity.Id == Shelf.Id;

  public bool IsDirty { get; private set; }

  public void MarkDirty()
  {
    IsDirty = true;
  }

  /// <summary>
  /// Create a brand new empty TileList, optionally using
  /// a predefined ID.
  /// </summary>
  /// <param name="id">
  /// The optional ID to use. If null, a new ID is generated.
  /// </param>
  /// <returns></returns>
  public static TileListModel Create(
    RackModel rack,
    TickId? id = null)
  {
    var entity = TileListData.CreateNew(id ?? TickId.New());
    return new TileListModel(rack, entity);
  }

  /// <summary>
  /// Load a TileList from a *.tile-list file in the given folder.
  /// </summary>
  /// <param name="store">
  /// The store in which to look for the file.
  /// </param>
  /// <param name="id">
  /// The ID, implying the file name.
  /// </param>
  /// <returns>
  /// Returns the list that was loaded, or null if the file
  /// did not exist.
  /// </returns>
  public static TileListModel? Load(
    RackModel rack,
    TickId id)
  {
    var entity = rack.Store.GetTiles(id);
    return entity == null ? null : new TileListModel(rack, entity);
  }
}
