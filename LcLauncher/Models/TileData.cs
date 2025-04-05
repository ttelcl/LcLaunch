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
public class TileData0: LaunchTile0
{
  public TileData0(
    TileGroup0? group = null,
    ShellLaunch? shellLaunch = null)
    : base(shellLaunch)
  {
    Group = group;
  }

  public static TileData0 EmptyTile()
  {
    return new TileData0();
  }

  public static TileData0 ShellTile(ShellLaunch shellLaunch)
  {
    return new TileData0(shellLaunch: shellLaunch);
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

  [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
  public TileGroup0? Group { get; set; }

}
