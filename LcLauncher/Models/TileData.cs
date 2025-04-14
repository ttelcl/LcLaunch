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

namespace LcLauncher.Models;

/// <summary>
/// Polymorphic data for tile content, containing one of
/// a few possible kinds of tile content. Only one of the
/// fields will be non-null (except for the quad field, which
/// can be an empty list as equivalent).
/// </summary>
public class TileData: LaunchTile
{
  public TileData(
    ShellLaunch? shellLaunch = null,
    RawLaunch? rawLaunch = null,
    TileGroup? group = null,
    IEnumerable<LaunchTile>? quad = null)
    : base(shellLaunch, rawLaunch)
  {
    Group = group;
    Quad = quad == null ? null : quad.ToList();
  }

  public static TileData EmptyTile()
  {
    return new TileData();
  }

  public static TileData ShellTile(ShellLaunch shellLaunch)
  {
    return new TileData(shellLaunch: shellLaunch);
  }

  public static TileData RawTile(RawLaunch rawLaunch)
  {
    return new TileData(rawLaunch: rawLaunch);
  }

  public static TileData GroupTile(TileGroup group)
  {
    return new TileData(group: group);
  }

  public static TileData GroupTile(
    Guid tilelist,
    string? title = null,
    string? tooltip = null)
  {
    return new TileData(group: new TileGroup(tilelist, title, tooltip));
  }

  public static TileData QuadTile(IEnumerable<LaunchTile> quad)
  {
    return new TileData(quad: quad);
  }

  [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
  public TileGroup? Group { get; set; }

  [JsonProperty("quad", NullValueHandling = NullValueHandling.Ignore)]
  public List<LaunchTile>? Quad { get; }

  //public bool ShouldSerializeQuad() => Quad.Count > 0;

}
