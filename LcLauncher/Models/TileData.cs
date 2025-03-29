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
public class TileData: LaunchTile
{
  public TileData(
    TileGroup? group = null,
    ShellLaunch? shellLaunch = null)
    : base(shellLaunch)
  {
    Group = group;
  }

  public static TileData EmptyTile()
  {
    return new TileData();
  }

  public static TileData ShellTile(ShellLaunch shellLaunch)
  {
    return new TileData(shellLaunch: shellLaunch);
  }

  public static TileData GroupTile(TileGroup group)
  {
    return new TileData(group: group);
  }

  public static TileData GroupTile(
    string title, IEnumerable<TileData> tiles)
  {
    return new TileData(group: new TileGroup(title, tiles));
  }

  [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
  public TileGroup? Group { get; set; }

}
