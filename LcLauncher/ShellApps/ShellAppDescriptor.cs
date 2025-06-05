/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Main;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;

namespace LcLauncher.ShellApps;

/// <summary>
/// Describes an item in the shell apps folder
/// </summary>
public class ShellAppDescriptor
{
  /// <summary>
  /// Create a new ShellAppDescriptor
  /// </summary>
  public ShellAppDescriptor(
    string name,
    string parsingName)
  {
    Name = name;
    if(HasShellAppsFolderPrefix(parsingName))
    {
      FullParsingName = parsingName;
      ParsingName = StripShellAppsPrefix(parsingName);
    }
    else
    {
      ParsingName = parsingName;
      FullParsingName = ShellAppsFolderPrefix + parsingName;
    }
  }

  public static ShellAppDescriptor FromShellObject(
    ShellObject shell)
  {
    return new ShellAppDescriptor(
      shell.Name,
      shell.ParsingName);
  }

  /// <summary>
  /// Create a Shell App Descriptor from an absolute or relative
  /// parsing name.
  /// </summary>
  public static ShellAppDescriptor? TryFromParsingName(
    string parsingName)
  {
    if(!HasShellAppsFolderPrefix(parsingName))
    {
      parsingName = ShellAppsFolderPrefix + parsingName;
    }
    try
    {
      using var shellObject = ShellObject.FromParsingName(parsingName);
      return FromShellObject(shellObject);
    }
    catch(ShellException)
    {
      return null;
    }
  }

  /// <summary>
  /// The friendly display name of the app
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// The parsing name relative to the apps folder
  /// </summary>
  public string ParsingName { get; }

  /// <summary>
  /// The absolute parsing name (including the apps folder)
  /// </summary>
  public string FullParsingName { get; }

  /// <summary>
  /// The prefix for the shell apps folder. Technically,
  /// this is case insensitive. Use <see cref="HasShellAppsFolderPrefix(string?)"/>
  /// to test for this prefix and its alternate form.
  /// </summary>
  public const string ShellAppsFolderPrefix =
    "shell:AppsFolder\\";

  public const string ShellAppsFolderPrefix2 =
    "shell:::{4234d49b-0245-4df3-B780-3893943456e1}\\";

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

  public static string StripShellAppsPrefix(string parsingName)
  {
    if(HasShellAppsFolderPrefix(parsingName))
    {
      var idx = parsingName.IndexOf('\\');
      if(idx < 0)
      {
        // Expecting anything that passes HasShellAppsFolderPrefix() to have at least one \
        throw new InvalidOperationException("Internal error");
      }
      return parsingName.Substring(idx+1);
    }
    else
    {
      return parsingName;
    }
  }
}
