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

namespace LcLauncher.DataModel.Entities;

/// <summary>
/// Stores a tile list (V3). 
/// </summary>
public class TileListData: IJsonStorable
{
  /// <summary>
  /// Deserialization constructor
  /// </summary>
  public TileListData(
    TickId id,
    IEnumerable<TileData?> tiles)
  {
    Id=id;
    Tiles = tiles.ToList();
  }

  /// <summary>
  /// Create a new tile list with 4 empty tiles and the given <paramref name="id"/>.
  /// </summary>
  public static TileListData CreateNew(TickId id)
  {
    return new TileListData(
      id, [
        TileData.EmptyTile(),
        TileData.EmptyTile(),
        TileData.EmptyTile(),
        TileData.EmptyTile(),
      ]);
  }

  /// <summary>
  /// Create a new tile list with 4 empty tiles and a newly generated id.
  /// </summary>
  public static TileListData CreateNew()
  {
    return CreateNew(TickId.New());
  }

  /// <summary>
  /// The ID of the tile list
  /// </summary>
  [JsonProperty("id")]
  public TickId Id { get; }

  /// <summary>
  /// The list of tiles
  /// </summary>
  public List<TileData?> Tiles { get; }
}
