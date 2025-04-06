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
/// fields will be non-null.
/// </summary>
public class TileData0: LaunchTile
{
  public TileData0(
    ShellLaunch? shellLaunch = null,
    RawLaunch? rawLaunch = null,
    TileGroup0? group = null,
    IEnumerable<LaunchTile>? quad = null)
    : base(shellLaunch, rawLaunch)
  {
    Group = group;
    Quad = new(quad ?? new List<LaunchTile>());
  }

  public static TileData0 EmptyTile()
  {
    return new TileData0();
  }

  public static TileData0 ShellTile(ShellLaunch shellLaunch)
  {
    return new TileData0(shellLaunch: shellLaunch);
  }

  public static TileData0 RawTile(RawLaunch rawLaunch)
  {
    return new TileData0(rawLaunch: rawLaunch);
  }

  public static TileData0 GroupTile(TileGroup0 group)
  {
    return new TileData0(group: group);
  }

  public static TileData0 GroupTile(
    string title, IEnumerable<TileData0> tiles)
  {
    return new TileData0(group: new TileGroup0(title, tiles));
  }

  public static TileData0 QuadTile(IEnumerable<LaunchTile> quad)
  {
    return new TileData0(quad: quad);
  }

  [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
  public TileGroup0? Group { get; set; }

  [JsonProperty("quad")]
  public List<LaunchTile> Quad { get; }

  public bool ShouldSerializeQuadTile => Quad.Count > 0;

}
