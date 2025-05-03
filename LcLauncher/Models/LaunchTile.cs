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

namespace LcLauncher.Models;

/// <summary>
/// Tile data for either kind of launch tile.
/// Only one of the fields will be non-null.
/// This is the base class for <see cref="TileData"/>,
/// and normally only used that way,
/// but also used as-is for the parts inside a quad tile.
/// </summary>
public class LaunchTile
{
  // reminder: once the merging of ShellLaunch and RawLaunch into
  // LaunchData is complete, this class can moved into TileData,
  // and QuadTile can use LaunchData instead of LaunchTile.
  public LaunchTile(
    ShellLaunch? shellLaunch = null,
    RawLaunch? rawLaunch = null,
    LaunchData? launch = null)
  {
    ShellLaunch = shellLaunch;
    RawLaunch = rawLaunch;
    Launch = launch;
  }

  /// <summary>
  /// Shell based launch tile.
  /// </summary>
  [JsonProperty("shellLaunch", NullValueHandling = NullValueHandling.Ignore)]
  public ShellLaunch? ShellLaunch { get; set; }

  [JsonProperty("rawLaunch", NullValueHandling = NullValueHandling.Ignore)]
  public RawLaunch? RawLaunch { get; set; }

  [JsonProperty("launch", NullValueHandling = NullValueHandling.Ignore)]
  public LaunchData? Launch { get; set; }

  public bool HasLaunch()
  {
    return ShellLaunch != null || RawLaunch != null || Launch != null;
  }
}
