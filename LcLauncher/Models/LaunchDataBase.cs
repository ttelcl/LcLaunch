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
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LcLauncher.Models;

/// <summary>
/// Abstract base class for both variants of launch tile
/// configuration data.
/// </summary>
public abstract class LaunchDataBase
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

  public string GetEffectiveTitle()
  {
    if(!String.IsNullOrEmpty(Title))
    {
      return Title;
    }
    return Path.GetFileNameWithoutExtension(TargetPath);
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
    return Path.GetFileName(TargetPath);
    //return null;
  }

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
  /// Get the effective icon source, based on <see cref="IconSource"/> and
  /// <see cref="TargetPath"/>.
  /// </summary>
  /// <returns></returns>
  public string GetIconSource()
  {
    return String.IsNullOrEmpty(IconSource) ? TargetPath : IconSource;
  }

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
      //if(File.Exists(target))
      //{
      //  // File exists
      //  return LaunchKind.Raw;
      //}
      //return LaunchKind.Missing;
    }
    else
    {
      if(target.StartsWith(LaunchDataBase.ShellAppsFolderPrefix))
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
        // Not a valid path, so not a document. Could still be a raw
        // shell app, but let's require the special prefix for that.
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
