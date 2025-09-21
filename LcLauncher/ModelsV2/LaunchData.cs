/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using LcLauncher.DataModel.Utilities;

using Model3 = LcLauncher.DataModel.Entities;

using PathEdit = LcLauncher.DataModel.Entities.PathEdit;

namespace LcLauncher.ModelsV2;

/// <summary>
/// New universal launch data design, merging the previous
/// <see cref="ShellLaunch"/> and <see cref="RawLaunch"/>.
/// </summary>
public class LaunchData
{
  public LaunchData(
    string target,
    bool shellmode,
    string? title = null,
    string? tooltip = null,
    ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
    string? iconSource = null,
    string? icon48 = null,
    string? icon32 = null,
    string? icon16 = null,
    string verb = "",
    string? workingDirectory = null,
    IEnumerable<string>? arguments = null,
    IDictionary<string, string?>? env = null,
    IDictionary<string, PathEdit>? pathenv = null)
  {
    Target = target;
    ShellMode = shellmode;
    Tooltip = tooltip;
    Title = title;
    WindowStyle = windowStyle;
    IconSource = iconSource;
    Icon48 = icon48;
    Icon32 = icon32;
    Icon16 = icon16;
    Verb = verb;
    WorkingDirectory = workingDirectory;
    Arguments = new(arguments ?? []);
    Environment = new(env ?? new Dictionary<string, string?>());
    PathEnvironment = [];
    foreach(var kv in pathenv ?? new Dictionary<string, PathEdit>())
    {
      // clone the PathEdit objects
      var edit = new PathEdit(
        kv.Value.Prepend,
        kv.Value.Append);
      PathEnvironment.Add(kv.Key, edit);
    }
  }

  /// <summary>
  /// The target. This can be a file, an URI, or an application tag
  /// (which is an URI, really, starting with "shell:AppsFolder\")
  /// </summary>
  [JsonProperty("target")]
  public string Target { get; set; }

  /// <summary>
  /// Shell launnch vs. raw launch. Greatly affects what valid values for
  /// other properties are. E.g., when false, the target must be an existing
  /// *.exe file. Or when true, environment variables are not supported and
  /// arguments are usually not supported, unless the target is an executable.
  /// </summary>
  [JsonProperty("shellmode")]
  public bool ShellMode { get; set; }

  /// <summary>
  /// The tile title. If null, the title will be inferred from the target.
  /// </summary>
  [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
  public string? Title { get; set; }

  [JsonProperty("tooltip", NullValueHandling = NullValueHandling.Ignore)]
  public string? Tooltip { get; set; }

  /// <summary>
  /// The startup window style. Default is Normal. Other options are
  /// Hidden, Minimized, and Maximized.
  /// </summary>
  [JsonProperty("windowStyle", DefaultValueHandling = DefaultValueHandling.Ignore)]
  [JsonConverter(typeof(StringEnumConverter))]
  [DefaultValue(ProcessWindowStyle.Normal)]
  public ProcessWindowStyle WindowStyle { get; set; }

  /// <summary>
  /// The file used to derive the icon for the tile.
  /// Usually null, in which case the icon will be derived from the target.
  /// </summary>
  [JsonProperty("iconSource", NullValueHandling = NullValueHandling.Ignore)]
  public string? IconSource { get; set; }

  /// <summary>
  /// The main icon ID.
  /// </summary>
  [JsonProperty("icon48", NullValueHandling = NullValueHandling.Ignore)]
  public string? Icon48 { get; set; }

  /// <summary>
  /// The medium icon ID.
  /// </summary>
  [JsonProperty("icon32", NullValueHandling = NullValueHandling.Ignore)]
  public string? Icon32 { get; set; }

  /// <summary>
  /// The small icon ID.
  /// </summary>
  [JsonProperty("icon16", NullValueHandling = NullValueHandling.Ignore)]
  public string? Icon16 { get; set; }

  /// <summary>
  /// The verb to use when launching the target. Default is empty (default action).
  /// Only valid when ShellMode is true.
  /// </summary>
  [JsonProperty("verb", DefaultValueHandling = DefaultValueHandling.Ignore,
    NullValueHandling = NullValueHandling.Ignore)]
  [DefaultValue("")]
  public string Verb { get; set; }

  [JsonProperty("workingDirectory", NullValueHandling = NullValueHandling.Ignore)]
  public string? WorkingDirectory { get; set; }

  [JsonProperty("arguments")]
  public List<string> Arguments { get; }

  public bool ShouldSerializeArguments() => Arguments.Count > 0;

  [JsonProperty("env")]
  public Dictionary<string, string?> Environment { get; }

  public bool ShouldSerializeEnvironment() => Environment.Count > 0;

  [JsonProperty("pathenv")]
  public Dictionary<string, PathEdit> PathEnvironment { get; }

  public bool ShouldSerializePathEnvironment() => PathEnvironment.Count > 0;

  public string GetEffectiveTitle()
  {
    if(!String.IsNullOrEmpty(Title))
    {
      return Title;
    }
    return Path.GetFileNameWithoutExtension(Target);
  }

  public string GetEffectiveTooltip()
  {
    if(!String.IsNullOrEmpty(Tooltip))
    {
      return Tooltip;
    }
    if(!String.IsNullOrEmpty(Title))
    {
      return Title;
    }
    return Path.GetFileName(Target);
    //return null;
  }

  /// <summary>
  /// Get the effective icon source, based on <see cref="IconSource"/> and
  /// <see cref="Target"/>.
  /// </summary>
  /// <returns></returns>
  public string GetIconSource()
  {
    return String.IsNullOrEmpty(IconSource) ? Target : IconSource;
  }


}
