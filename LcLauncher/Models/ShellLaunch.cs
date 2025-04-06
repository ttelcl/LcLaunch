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
  "target": "C:\\Windows\\System32\\@WLOGO_48x48.png",
  "iconSource": null,
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
public class ShellLaunch: LaunchData
{
  public ShellLaunch(
    string target,
    string? tooltip = null,
    string? title = null,
    ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
    string? iconSource = null,
    string? icon48 = null,
    string? icon32 = null,
    string? icon16 = null,
    string verb = "")
    : base(target, tooltip, title, windowStyle,
           iconSource, icon48, icon32, icon16)
  {
    Verb = verb;
  }

  /// <summary>
  /// The verb to use when launching the target. Default is empty (default action).
  /// Rarely used, but available for unusual cases. Valid values depend on the
  /// target's file type!
  /// </summary>
  [JsonProperty("verb", DefaultValueHandling = DefaultValueHandling.Ignore,
    NullValueHandling = NullValueHandling.Ignore)]
  [DefaultValue("")]
  public string Verb { get; set; }
}
