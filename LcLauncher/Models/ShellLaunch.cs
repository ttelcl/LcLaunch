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
/// Tile content for traditional application launcher,
/// using the shell to launch it.
/// </summary>
public class ShellLaunch
{
  public ShellLaunch(
    string target,
    string? tooltip = null,
    string? title = null)
  {
    TargetPath = target;
    Tooltip = tooltip;
    Title = title;
  }

  [JsonProperty("target")]
  public string TargetPath { get; set; }

  [JsonProperty("tooltip", NullValueHandling = NullValueHandling.Ignore)]
  public string? Tooltip { get; set; }

  /// <summary>
  /// The tile title. If null, the title will be inferred from the target.
  /// </summary>
  [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
  public string? Title { get; set; }
}
