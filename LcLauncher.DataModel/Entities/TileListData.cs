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
  /// The ID of the tile list
  /// </summary>
  [JsonProperty("id")]
  public TickId Id { get; }

  /// <summary>
  /// The list of tiles
  /// </summary>
  public List<TileData?> Tiles { get; }
}
