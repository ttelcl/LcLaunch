/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcLauncher.DataModel.Utilities;

/// <summary>
/// Static functions related to Shell App information that do not require
/// the Shell App library
/// </summary>
public static class ShellAppTools
{

  /// <summary>
  /// The prefix for the shell apps folder. Technically,
  /// this is case insensitive.
  /// Use <see cref="HasShellAppsFolderPrefix(string?)"/>
  /// to test for this prefix and its alternate form.
  /// </summary>
  public const string ShellAppsFolderPrefix =
    "shell:AppsFolder\\";

  /// <summary>
  /// Like <see cref="ShellAppsFolderPrefix"/>, but in GUID form
  /// </summary>
  public const string ShellAppsFolderPrefix2 =
    "shell:::{4234d49b-0245-4df3-B780-3893943456e1}\\";

  /// <summary>
  /// Test if the launch <paramref name="target"/> starts with a prefix
  /// indicating a Shell App (<see cref="ShellAppsFolderPrefix"/> or
  /// <see cref="ShellAppsFolderPrefix2"/>). Case insensitive.
  /// </summary>
  public static bool HasShellAppsFolderPrefix(string? target)
  {
    if(String.IsNullOrEmpty(target)
      || !target.StartsWith("shell:")) // case sensitive!
    {
      return false;
    }
    return
      target.StartsWith(
        ShellAppsFolderPrefix,
        StringComparison.InvariantCultureIgnoreCase)
      || target.StartsWith(
        ShellAppsFolderPrefix2,
        StringComparison.InvariantCultureIgnoreCase);
  }

  /// <summary>
  /// Return <paramref name="parsingName"/>, but if it starts with a
  /// shell app prefix, remove that prefix.
  /// </summary>
  public static string StripShellAppsPrefix(string parsingName)
  {
    if(HasShellAppsFolderPrefix(parsingName))
    {
      var idx = parsingName.IndexOf('\\');
      if(idx < 0)
      {
        // Expecting anything that passes HasShellAppsFolderPrefix() to have
        // at least one \
        throw new InvalidOperationException("Internal error");
      }
      return parsingName.Substring(idx+1);
    }
    else
    {
      return parsingName;
    }
  }

  /// <summary>
  /// Return <paramref name="parsingName"/>, but if it does not start with
  /// a shell app prefix, prepend that.
  /// </summary>
  public static string WithShellAppsPrefix(string parsingName)
  {
    if(!HasShellAppsFolderPrefix(parsingName))
    {
      return ShellAppsFolderPrefix + parsingName;
    }
    else
    {
      return parsingName;
    }
  }

}
