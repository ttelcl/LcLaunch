/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.Models;

/// <summary>
/// Temporary common interface for old and new launch data models.
/// </summary>
public interface ILaunchData
{
  string TargetPath { get; set; }
  string? Title { get; set; }
  string? Tooltip { get; set; }
  string? IconSource { get; set; }
  string? Icon48 { get; set; }
  string? Icon32 { get; set; }
  string? Icon16 { get; set; }
  public bool ShellMode { get; }
}

public interface IShellLaunchData: ILaunchData
{
  string Verb { get; set; }
}

public interface IRawLaunchData: ILaunchData
{
  string? WorkingDirectory { get; set; }
  List<string> Arguments { get; }
  Dictionary<string, string?> Environment { get; }
  Dictionary<string, PathEdit> PathEnvironment { get; }
}


public static class LaunchDataExtensions
{

  public static string GetEffectiveTitle(this ILaunchData data)
  {
    if(!String.IsNullOrEmpty(data.Title))
    {
      return data.Title;
    }
    return
      data.TargetLooksLikeFile()
      ? Path.GetFileNameWithoutExtension(data.TargetPath)
      : "Untitled";
  }

  public static string GetEffectiveTooltip(this ILaunchData data)
  {
    if(!String.IsNullOrEmpty(data.Tooltip))
    {
      return data.Tooltip;
    }
    if(!String.IsNullOrEmpty(data.Title))
    {
      return data.Title;
    }
    return
      data.TargetLooksLikeFile()
      ? Path.GetFileName(data.TargetPath)
      : data.TargetPath;
    //return null;
  }

  /// <summary>
  /// Get the effective icon source, based on <see cref="IconSource"/> and
  /// <see cref="TargetPath"/>.
  /// </summary>
  /// <returns></returns>
  public static string GetIconSource(this ILaunchData data)
  {
    return String.IsNullOrEmpty(data.IconSource)
      ? data.TargetPath
      : data.IconSource;
  }

  public static bool TargetLooksLikeFile(this ILaunchData data)
  {
    var target = data.TargetPath;
    if(String.IsNullOrEmpty(target) 
      || target.Length < 6
      || target[1] != ':')
    {
      return false;
    }
    if((target[0] < 'A' || target[0] > 'Z')
      && (target[0] < 'a' || target[0] > 'z'))
    {
      return false;
    }
    if(target[2] != Path.DirectorySeparatorChar
      && target[2] != Path.AltDirectorySeparatorChar)
    {
      return false;
    }
    return true;
  }

}
