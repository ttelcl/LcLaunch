/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ttelcl.Persistence.API;

namespace LcLauncher.DataModel.Entities;

/// <summary>
/// Polymorphic data for tile content, containing one of
/// a few possible kinds of tile content. Only one of the
/// fields will be non-null (except for the quad field, which
/// can be an empty list as equivalent).
/// A list of these is what gets serialized in tile list files.
/// </summary>
public class TileData
{
  /// <summary>
  /// Deserialization constructor
  /// </summary>
  /// <param name="launch"></param>
  /// <param name="group"></param>
  /// <param name="quad"></param>
  public TileData(
    LaunchData? launch = null,
    GroupData? group = null,
    IEnumerable<LaunchData?>? quad = null)
  {
    Launch = launch;
    Group = group;
    Quad = quad == null ? null : quad.ToList();
  }

  /// <summary>
  /// Create a new <see cref="TileData"/> representing an 
  /// empty tile
  /// </summary>
  public static TileData EmptyTile()
  {
    return new TileData();
  }

  /// <summary>
  /// Create a new <see cref="TileData"/> representing a
  /// launcher tile
  /// </summary>
  public static TileData LaunchTile(LaunchData launch)
  {
    return new TileData(
      launch: launch);
  }

  /// <summary>
  /// Create a new <see cref="TileData"/> representing a
  /// group tile
  /// </summary>
  public static TileData GroupTile(GroupData group)
  {
    return new TileData(group: group);
  }

  /// <summary>
  /// Create a new <see cref="TileData"/> representing a
  /// quad tile (containing 0 - 4 small tiles)
  /// </summary>
  public static TileData QuadTile(IEnumerable<LaunchData?> quad)
  {
    return new TileData(quad: quad);
  }

  /// <summary>
  /// The launch data in case this is a launcher tile
  /// </summary>
  [JsonProperty("launch", NullValueHandling = NullValueHandling.Ignore)]
  public LaunchData? Launch { get; }

  /// <summary>
  /// True if there is a non-null <see cref="Launch"/> data
  /// </summary>
  /// <returns></returns>
  public bool HasLaunch()
  {
    return Launch != null;
  }

  /// <summary>
  /// The group data in case this is a group tile
  /// </summary>
  [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
  public GroupData? Group { get; }

  /// <summary>
  /// The quad data in case this is a quad tile. 
  /// </summary>
  [JsonProperty("quad", NullValueHandling = NullValueHandling.Ignore)]
  public List<LaunchData?>? Quad { get; }

  /// <summary>
  /// True if this is an empty tile, without specialized tile data.
  /// </summary>
  /// <returns></returns>
  public bool IsEmpty()
  {
    return
      Launch == null &&
      Group == null &&
      Quad == null;
  }

}
