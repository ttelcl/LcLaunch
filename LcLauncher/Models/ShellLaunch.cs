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
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace LcLauncher.Models;

/* JSON examples:
 * Note that this class is primarily for launching documents,
 * not so much executables. Although those work too.
 
{
  "target": "c:\\Windows\\System32\\@WLOGO_48x48.png",
  "tooltip": "Windows logo",
  "title": "Logo image",
  "windowStyle": "Normal",
  "verb": "",
  "arguments": []
}

 * Most fields are optional. Only 'target' is required.

{
  "target": "c:\\Windows\\System32\\@WLOGO_48x48.png"
}

*/

/// <summary>
/// Tile content for traditional application launcher,
/// using the shell to launch it.
/// </summary>
public class ShellLaunch
{
  public ShellLaunch(
    string target,
    string? tooltip = null,
    string? title = null,
    ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
    string verb = "",
    IEnumerable<string>? arguments = null)
  {
    TargetPath = target;
    Tooltip = tooltip;
    Title = title;
    WindowStyle = windowStyle;
    Verb = verb;
    Arguments = arguments?.ToList() ?? [];
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

  /// <summary>
  /// The startup window style. Default is Normal. Other options are
  /// Hidden, Minimized, and Maximized.
  /// </summary>
  [JsonProperty("windowStyle", DefaultValueHandling = DefaultValueHandling.Ignore)]
  [JsonConverter(typeof(StringEnumConverter))]
  [DefaultValue(ProcessWindowStyle.Normal)]
  public ProcessWindowStyle WindowStyle { get; set; }

  /// <summary>
  /// The verb to use when launching the target. Default is empty (default action).
  /// Rarely used, but available for unusual cases. Valid values depend on the
  /// target's file type!
  /// </summary>
  [JsonProperty("verb", DefaultValueHandling = DefaultValueHandling.Ignore)]
  [DefaultValue("")]
  public string Verb { get; set; }

  [JsonProperty("arguments")]
  public List<string> Arguments { get; set; }

  public bool ShouldSerializeArguments()
  {
    return Arguments.Count > 0;
  }
}
