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
/// </summary>
public class LaunchTile
{
  public LaunchTile(
    ShellLaunch? shellLaunch = null)
  {
    ShellLaunch = shellLaunch;
  }

  /// <summary>
  /// Shell based launch tile.
  /// </summary>
  [JsonProperty("shellLaunch", NullValueHandling = NullValueHandling.Ignore)]
  public ShellLaunch? ShellLaunch { get; set; }

  // Non-shell launch tile field will be added here.
}
