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

namespace LcLauncher.Models;

public class ShelfModel
{
  private ShelfModel(
    RackModel owner,
    Guid id,
    ShelfData shelf)
  {
    Owner = owner;
    Id = id;
    Shelf = shelf;
    // TODO: initialize the primary tile list
  }

  public static ShelfModel Load(
    RackModel owner,
    Guid id)
  {
    var shelf =
      owner.Store.LoadData<ShelfData>(id, ".shelf-json");
    if(shelf == null)
    {
      shelf = new ShelfData(
        "MISSING Shelf " + id.ToString(),
        "Cobalt");
    }
    if(String.IsNullOrEmpty(shelf.Title))
    {
      shelf.Title = "Untitled Shelf " + id.ToString();
    }
    return new ShelfModel(owner, id, shelf);
  }

  public RackModel Owner { get; }

  public Guid Id { get; }

  public ShelfData Shelf { get; }

}
