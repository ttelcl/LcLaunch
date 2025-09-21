/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Ttelcl.Persistence.API;

using LcLauncher.Persistence;

using Model2 = LcLauncher.ModelsV2;
using Model3 = LcLauncher.DataModel.Entities;

namespace LcLauncher.Models;

public class ShelfModel
{
  internal ShelfModel(
    RackModel owner,
    Guid id,
    Model2.ShelfData shelf)
  {
    Owner = owner;
    Id = id;
    Shelf = shelf;
    var tilesModel = TileListModel.Load(Owner, id);
    if(tilesModel == null)
    {
      tilesModel = TileListModel.Create(Owner, id);
      tilesModel.SaveRawModel();
    }
    PrimaryTiles = tilesModel;
    IsDirty = false;
  }

  public static ShelfModel Load(
    RackModel owner,
    Guid id)
  {
    var shelf = owner.Store.LoadShelf(id);
    bool needsSave = false;
    if(shelf == null)
    { 
      shelf = new Model2.ShelfData(
        "MISSING Shelf " + id.ToString(),
        true, // Start out collapsed in this unusual case
        "Cobalt",
        id,
        TickId.New());
      needsSave = true;
    }
    if(String.IsNullOrEmpty(shelf.Title))
    {
      shelf.Title = "Untitled Shelf " + id.ToString();
    }
    if(shelf.IdOld != id)
    {
      shelf.IdOld = id;
      shelf.Tid = TickId.New();
      needsSave = true;
    }
    if(needsSave)
    {
      owner.Store.SaveShelf(id, shelf);
    }
    return new ShelfModel(owner, id, shelf);
  }

  public RackModel Owner { get; }

  public ILcLaunchStore Store => Owner.Store;

  public Guid Id { get; }

  public Model2.ShelfData Shelf { get; }

  public TileListModel PrimaryTiles { get; }

  public void Save()
  {
    Store.SaveShelf(Id, Shelf);
    IsDirty = false;
  }

  public bool IsDirty { get; private set; }

  public void MarkDirty()
  {
    IsDirty = true;
  }

}
