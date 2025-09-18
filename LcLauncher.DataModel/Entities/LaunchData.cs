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

using LcLauncher.DataModel.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Ttelcl.Persistence.API;

namespace LcLauncher.DataModel.Entities;

/// <summary>
/// Launch tile data (covering multiple variants at once)
/// </summary>
public class LaunchData
{
  /// <summary>
  /// Deserialization constructor
  /// </summary>
  public LaunchData(
    string target,
    bool shellmode,
    string? title = null,
    string? tooltip = null,
    ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
    string? iconSource = null,
    HashId? icon48 = null,
    HashId? icon32 = null,
    HashId? icon16 = null,
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
  /// Shell launch vs. raw launch. Greatly affects what valid values for
  /// other properties are. E.g., when false, the target must be an existing
  /// *.exe file. Or when true, environment variables are not supported and
  /// arguments are usually not supported, unless the target is an executable.
  /// </summary>
  [JsonProperty("shellmode")]
  public bool ShellMode { get; set; }

  /// <summary>
  /// The tile title. If null, the title will be inferred from 
  /// <see cref="Target"/>.
  /// </summary>
  [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
  public string? Title { get; set; }

  /// <summary>
  /// The tooltip. If null, the tooltip will be derived from 
  /// <see cref="Title"/> or <see cref="Target"/>
  /// </summary>
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
  public HashId? Icon48 { get; set; }

  /// <summary>
  /// The medium icon ID.
  /// </summary>
  [JsonProperty("icon32", NullValueHandling = NullValueHandling.Ignore)]
  public HashId? Icon32 { get; set; }

  /// <summary>
  /// The small icon ID.
  /// </summary>
  [JsonProperty("icon16", NullValueHandling = NullValueHandling.Ignore)]
  public HashId? Icon16 { get; set; }

  /// <summary>
  /// The verb to use when launching the target. Default is empty (default action).
  /// Only valid when ShellMode is true.
  /// </summary>
  [JsonProperty("verb", DefaultValueHandling = DefaultValueHandling.Ignore,
    NullValueHandling = NullValueHandling.Ignore)]
  [DefaultValue("")]
  public string Verb { get; set; }

  /// <summary>
  /// The working directory
  /// </summary>
  [JsonProperty("workingDirectory", NullValueHandling = NullValueHandling.Ignore)]
  public string? WorkingDirectory { get; set; }

  /// <summary>
  /// The arguments
  /// </summary>
  [JsonProperty("arguments")]
  public List<string> Arguments { get; }

  /// <summary>
  /// Returns true if <see cref="Arguments"/> should be serialized.
  /// False if there are no arguments.
  /// </summary>
  public bool ShouldSerializeArguments() => Arguments.Count > 0;

  /// <summary>
  /// Environment variables to set or override or remove in their entirety.
  /// </summary>
  [JsonProperty("env")]
  public Dictionary<string, string?> Environment { get; }

  /// <summary>
  /// Returns true if <see cref="Environment"/> should be serialized.
  /// False if there are no environment variables
  /// </summary>
  public bool ShouldSerializeEnvironment() => Environment.Count > 0;

  /// <summary>
  /// Edits to PATH-like environment variables, allowing to prepend
  /// or append items to such lists (using the platform-dependent
  /// separator, i.e. ';' on Windows)
  /// </summary>
  [JsonProperty("pathenv")]
  public Dictionary<string, PathEdit> PathEnvironment { get; }

  /// <summary>
  /// Returns true if <see cref="PathEnvironment"/> should be serialized.
  /// False if there are no edits stored in it.
  /// </summary>
  public bool ShouldSerializePathEnvironment() => PathEnvironment.Count > 0;

  /// <summary>
  /// Return <see cref="Title"/> if not empty, or derive a title from
  /// <see cref="Target"/>.
  /// </summary>
  /// <returns></returns>
  public string GetEffectiveTitle()
  {
    if(!String.IsNullOrEmpty(Title))
    {
      return Title;
    }
    return Path.GetFileNameWithoutExtension(Target);
  }

  /// <summary>
  /// Return <see cref="Tooltip"/> if not empty, or derive a default
  /// from <see cref="Title"/> or <see cref="Target"/>.
  /// </summary>
  /// <returns></returns>
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
  }

  /// <summary>
  /// Get the effective icon source, based on <see cref="IconSource"/> and
  /// <see cref="Target"/>.
  /// </summary>
  public string GetIconSource()
  {
    return String.IsNullOrEmpty(IconSource) ? Target : IconSource;
  }

  /// <summary>
  /// Determine the <paramref name="target"/>'s launch kind
  /// (using heuristics)
  /// </summary>
  /// <param name="target">
  /// The target file / app / uri to classify
  /// </param>
  /// <param name="raw">
  /// If true, we expect <paramref name="target"/> to be a straight
  /// executable file. This corresponds to <see cref="LaunchData.ShellMode"/>
  /// (inverted).
  /// </param>
  /// <returns>
  /// One of the <see cref="LaunchKind"/> values.
  /// </returns>
  public static LaunchKind GetLaunchKind(
    string target, bool raw)
  {
    if(String.IsNullOrEmpty(target))
    {
      return LaunchKind.Invalid;
    }
    if(raw)
    {
      if(!target.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
      {
        return LaunchKind.Invalid;
      }
      if(target.Length < 8
        || target[1] != ':'
        || Char.ToUpper(target[0]) < 'A'
        || Char.ToUpper(target[0]) > 'Z'
        || (target[2] != Path.DirectorySeparatorChar &&
            target[2] != Path.AltDirectorySeparatorChar))
      {
        return LaunchKind.Invalid;
      }
      // Looks like a path. At this point we don't care if it exists
      // (too expensive to check here)
      return LaunchKind.Raw;
    }
    else
    {
      if(ShellAppTools.HasShellAppsFolderPrefix(target))
      {
        // This is definitely a shell app. It may be missing, but
        // that's something to figure out later.
        return LaunchKind.ShellApplication;
      }
      if(target.Length < 8
        || target[1] != ':'
        || Char.ToUpper(target[0]) < 'A'
        || Char.ToUpper(target[0]) > 'Z'
        || (target[2] != Path.DirectorySeparatorChar &&
            target[2] != Path.AltDirectorySeparatorChar))
      {
        // test if it looks like an URI
        var colonIndex = target.IndexOf(':');
        if(colonIndex >= 2)
        {
          // Scheme does allow upper case letters according to RFC 3986,
          // but is case insensitive, and should be lower case.
          // We require a minimum of 2 characters for the scheme so we
          // can distinguish between a scheme and a drive letter.
          if(Regex.IsMatch(
              target.Substring(0, colonIndex+1),
              @"^[a-zA-Z][-+a-zA-Z0-9.]+:$"))
          {
            // Looks like a URI
            return LaunchKind.UriKind;
          }
        }

        // Not a valid path, so not a document. Not an URI either. Could
        // still be a raw shell app, but let's require the special prefix
        // for that.
        return LaunchKind.Invalid;
      }
      else
      {
        // Looks like a path. At this point we don't care if it exists
        // (too expensive to check here)
        return LaunchKind.Document;
      }
    }
  }
}
