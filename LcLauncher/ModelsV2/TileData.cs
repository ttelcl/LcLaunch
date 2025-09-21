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

namespace LcLauncher.ModelsV2;

/// <summary>
/// Polymorphic data for tile content, containing one of
/// a few possible kinds of tile content. Only one of the
/// fields will be non-null (except for the quad field, which
/// can be an empty list as equivalent).
/// A list of these is what gets serialized in tile list files.
/// </summary>
public class TileData
{
  public TileData(
    LaunchData? launch = null,
    GroupData? group = null,
    IEnumerable<LaunchData?>? quad = null)
  {
    Launch = launch;
    Group = group;
    Quad = quad == null ? null : quad.ToList();
  }

  public static TileData EmptyTile()
  {
    return new TileData();
  }

  public static TileData LaunchTile(LaunchData launch)
  {
    return new TileData(
      launch: launch);
  }

  public static TileData GroupTile(GroupData group)
  {
    return new TileData(group: group);
  }

  public static TileData GroupTile(
    Guid tilelist,
    string? title = null,
    string? tooltip = null)
  {
    return new TileData(group: new GroupData(tilelist, title, tooltip));
  }

  public static TileData QuadTile(IEnumerable<LaunchData?> quad)
  {
    return new TileData(quad: quad);
  }

  [JsonProperty("launch", NullValueHandling = NullValueHandling.Ignore)]
  public LaunchData? Launch { get; }

  public bool HasLaunch()
  {
    return Launch != null;
  }

  [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
  public GroupData? Group { get; }

  [JsonProperty("quad", NullValueHandling = NullValueHandling.Ignore)]
  public List<LaunchData?>? Quad { get; }

  public bool IsEmpty()
  {
    return
      Launch == null &&
      Group == null &&
      Quad == null;
  }

}
