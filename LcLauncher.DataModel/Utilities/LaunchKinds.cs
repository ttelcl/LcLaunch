/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LcLauncher.DataModel.Utilities;

/// <summary>
/// Static utilities related to <see cref="LaunchKind"/>
/// </summary>
public static class LaunchKinds
{
  /// <summary>
  /// Given the execution target text and a raw mode flag, determine
  /// the best matching <see cref="LaunchKind"/>
  /// </summary>
  /// <param name="target">
  /// The execution target: a file, a web URI, some other URI, or something
  /// else that <see cref="Process"/> can start.
  /// </param>
  /// <param name="raw">
  /// The raw flag. "raw" here means: its a plain executable file,
  /// not shell mode.
  /// </param>
  /// <returns>
  /// The best fitting <see cref="LaunchKind"/> (<see cref="LaunchKind.Invalid"/>
  /// on failure)
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
        || target[2] != Path.DirectorySeparatorChar &&
            target[2] != Path.AltDirectorySeparatorChar)
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
        || target[2] != Path.DirectorySeparatorChar &&
            target[2] != Path.AltDirectorySeparatorChar)
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
