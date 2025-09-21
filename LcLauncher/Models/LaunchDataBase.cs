/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using LcLauncher.ShellApps;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LcLauncher.Models;

#if MODEL2

/// <summary>
/// Abstract base class for both variants of launch tile
/// configuration data. DEPRECATED
/// </summary>
public abstract class LaunchDataBase: ILaunchData
{
  /// <summary>
  /// Create a new LaunchData
  /// </summary>
  protected LaunchDataBase(
    string target,
    string? title = null,
    string? tooltip = null,
    ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
    string? iconSource = null,
    string? icon48 = null,
    string? icon32 = null,
    string? icon16 = null)
  {
    TargetPath = target;
    Tooltip = tooltip;
    Title = title;
    WindowStyle = windowStyle;
    IconSource = iconSource;
    Icon48 = icon48;
    Icon32 = icon32;
    Icon16 = icon16;
  }

  /// <summary>
  /// The target path (the file to be launched). Which values are
  /// valid depends on the subclass. <see cref="ShellLaunch"/> can
  /// launch pretty much anything, while <see cref="RawLaunch"/>
  /// can only launch *.exe files.
  /// </summary>
  [JsonProperty("target")]
  public string TargetPath { get; set; }

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

  [JsonIgnore]
  public abstract bool ShellMode { get; }

  public const string ShellAppsFolderPrefix =
    "shell:AppsFolder\\";

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
      if(ShellAppDescriptor.HasShellAppsFolderPrefix(target))
      {
        // This is deffinitely a shell app. It may be missing, but
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
              target.Substring(0, colonIndex),
              @"^[a-zA-Z][-+a-zA-Z0-9.]+:"))
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

#endif
