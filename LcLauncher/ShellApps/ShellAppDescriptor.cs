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
using System.Windows.Media.Imaging;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

using LcLauncher.Main;
using LcLauncher.Models;
using LcLauncher.DataModel.Utilities;

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
    string label,
    string parsingName)
  {
    Label = label;
    if(ShellAppTools.HasShellAppsFolderPrefix(parsingName))
    {
      FullParsingName = parsingName;
      ParsingName = ShellAppTools.StripShellAppsPrefix(parsingName);
    }
    else
    {
      ParsingName = parsingName;
      FullParsingName = ShellAppTools.ShellAppsFolderPrefix + parsingName;
    }
    Kind = ShellAppKind.Unclassified;
    Classify();
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
    parsingName = ShellAppTools.WithShellAppsPrefix(parsingName);
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

  [JsonConverter(typeof(StringEnumConverter))]
  [JsonProperty("kind")]
  public ShellAppKind Kind { get; private set; }

  /// <summary>
  /// The parsing name relative to the apps folder
  /// </summary>
  [JsonProperty("parsingname")]
  public string ParsingName { get; }

  /// <summary>
  /// The friendly display name of the app
  /// </summary>
  [JsonProperty("label")]
  public string Label { get; }

  /// <summary>
  /// The absolute parsing name (including the apps folder)
  /// </summary>
  [JsonIgnore]
  public string FullParsingName { get; }

  [JsonProperty("path")]
  public string? FileSystemPath { get; private set; }

  public bool ShouldSerializeFileSystemPath()
  {
    return FileSystemPath != null; 
  }

  [JsonIgnore]
  public AppIdLike? AppId { get; private set; }

  /// <summary>
  /// True if <see cref="FileSystemPath"/> points to an existing file
  /// (and not a folder, nor is undefined)
  /// </summary>
  [JsonIgnore]
  public bool HasFile {
    get {
      return
        Kind switch {
          ShellAppKind.PlainFileApp => FileSystemPath != null,
          ShellAppKind.FolderFileApp => FileSystemPath != null,
          _ => false
        };
    }
  }

  /// <summary>
  /// True if <see cref="FileSystemPath"/> points to an existing folder
  /// (and not a file, nor is undefined). This is a very unusual case.
  /// </summary>
  [JsonIgnore]
  public bool HasFolder {
    get {
      return
        Kind switch {
          ShellAppKind.PlainFolderApp => FileSystemPath != null,
          _ => false
        };
    }
  }

  /// <summary>
  /// True if this app is an accessible file that is an executable.
  /// If true, this app can be used by raw tiles, doc tiles and app tiles.
  /// </summary>
  [JsonIgnore]
  public bool IsExe {
    get => HasFile && FileSystemPath!.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
  }

  /// <summary>
  /// True if this app is an accessible file that is NOT an executable.
  /// If true, this app can be used by doc tiles and app tiles, but not by raw tiles.
  /// </summary>
  [JsonIgnore]
  public bool IsDoc {
    get => HasFile && !FileSystemPath!.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
  }

  /// <summary>
  /// True if this app is an accessible folder.
  /// If true, this app can be used by doc tiles and app tiles, but not by raw tiles.
  /// </summary>
  [JsonIgnore]
  public bool IsFolder {
    get => HasFolder;
  }

  /// <summary>
  /// True if this app can be launched directly as URI, without being wrapped as an app
  /// first.
  /// </summary>
  [JsonIgnore]
  public bool IsUri {
    get => Kind == ShellAppKind.UriKind;
  }

  private void Classify()
  {
    // WIP!
    if(ParsingName.Length < 6)
    {
      Kind = ShellAppKind.Other;
      return;
    }
    if(ParsingName[1] == ':')
    {
      var drive = Char.ToUpper(ParsingName[0]);
      if(drive >= 'A' && drive <= 'Z')
      {
        // looks like a plain file name
        if(File.Exists(ParsingName))
        {
          Kind = ShellAppKind.PlainFileApp;
          FileSystemPath = ParsingName;
          return;
        }
        else if(Directory.Exists(ParsingName))
        {
          Kind = ShellAppKind.PlainFolderApp;
          FileSystemPath = ParsingName;
          return;
        }
        else
        {
          Trace.TraceWarning($"File-like parsing name does not exist: {ParsingName}");
          Kind = ShellAppKind.Other;
          return;
        }
      }
    }
    if(ParsingName[0] == '{')
    {
      var fileName = KnownFolderMap.ExpandKnownFolder(ParsingName);
      if(fileName[0] != '{')
      {
        Kind = ShellAppKind.FolderFileApp;
        FileSystemPath = fileName;
        return;
      }
      // else: fall through
    }
    if(ParsingName.StartsWith("Microsoft.AutoGenerated."))
    {
      Kind = ShellAppKind.Autogenerated;
      return;
    }
    var appIdLike = AppIdLike.TryParse(ParsingName);
    if(appIdLike != null)
    {
      Kind = ShellAppKind.AppId;
      AppId = appIdLike;
      return;
    }
    if(ParsingName.IndexOf(':') >= 2)
    {
      if(Regex.IsMatch(
              ParsingName,
              @"^[a-zA-Z][-+a-zA-Z0-9.]+:"))
      {
        Kind = ShellAppKind.UriKind;
        return;
      }
    }
    if(Regex.IsMatch(
      ParsingName,
      @"^[a-z][a-z0-9]+([-_][a-z0-9]+)*(\.[a-z0-9]+([-_][a-z0-9]+)*)*$",
      RegexOptions.IgnoreCase))
    {
      Kind = ShellAppKind.DottedName;
      return;
    }
    Kind = ShellAppKind.Other;
  }

  ///// <summary>
  ///// The prefix for the shell apps folder. Technically,
  ///// this is case insensitive. Use <see cref="HasShellAppsFolderPrefix(string?)"/>
  ///// to test for this prefix and its alternate form.
  ///// </summary>
  //public const string ShellAppsFolderPrefix =
  //  "shell:AppsFolder\\";

  //public const string ShellAppsFolderPrefix2 =
  //  "shell:::{4234d49b-0245-4df3-B780-3893943456e1}\\";

  //public static bool HasShellAppsFolderPrefix(string? target)
  //{
  //  if(String.IsNullOrEmpty(target)
  //    || !target.StartsWith("shell:")) // case sensitive!
  //  {
  //    return false;
  //  }
  //  return
  //    target.StartsWith(
  //      ShellAppsFolderPrefix,
  //      StringComparison.InvariantCultureIgnoreCase)
  //    || target.StartsWith(
  //      ShellAppsFolderPrefix2,
  //      StringComparison.InvariantCultureIgnoreCase);
  //}

  //public static string StripShellAppsPrefix(string parsingName)
  //{
  //  if(HasShellAppsFolderPrefix(parsingName))
  //  {
  //    var idx = parsingName.IndexOf('\\');
  //    if(idx < 0)
  //    {
  //      // Expecting anything that passes HasShellAppsFolderPrefix() to have at least one \
  //      throw new InvalidOperationException("Internal error");
  //    }
  //    return parsingName.Substring(idx+1);
  //  }
  //  else
  //  {
  //    return parsingName;
  //  }
  //}

  //public static string WithShellAppsPrefix(string parsingName)
  //{
  //  if(!HasShellAppsFolderPrefix(parsingName))
  //  {
  //    return ShellAppsFolderPrefix + parsingName;
  //  }
  //  else
  //  {
  //    return parsingName;
  //  }
  //}

  [JsonIgnore]
  public BitmapSource? Icon { get; set; }
}
