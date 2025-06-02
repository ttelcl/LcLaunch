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
/// (To be deprecated!) Tile content for traditional application launcher,
/// using the shell to launch it.
/// </summary>
public class ShellLaunch: LaunchDataBase, IShellLaunchData
{
  public ShellLaunch(
    string target,
    string? title = null,
    string? tooltip = null,
    ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
    string? iconSource = null,
    IEnumerable<string>? arguments = null,
    string? icon48 = null,
    string? icon32 = null,
    string? icon16 = null,
    string verb = "")
    : base(target, title, tooltip, windowStyle,
           iconSource, icon48, icon32, icon16)
  {
    Verb = verb;
    Arguments = new(arguments ?? []);
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

  /// <summary>
  /// The arguments. Only valid when the target is an executable.
  /// </summary>
  [JsonProperty("arguments")]
  public List<string> Arguments { get; }

  public bool ShouldSerializeArguments() => Arguments.Count > 0;

  [JsonIgnore]
  public override bool ShellMode => true;
}
