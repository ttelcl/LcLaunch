/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.WindowsAPICodePack.Shell;

namespace LcLauncher.ShellApps;

/// <summary>
/// A mapping of folder GUIDs to their 'known folder'
/// information
/// </summary>
public class KnownFolderMap
{
  private readonly Dictionary<Guid, IKnownFolder> _folderMap;

  /// <summary>
  /// Create the new KnownFolderMap instance
  /// </summary>
  private KnownFolderMap()
  {
    _folderMap = new Dictionary<Guid, IKnownFolder>();
    foreach(var knownFolder in KnownFolders.All)
    {
      _folderMap[knownFolder.FolderId] = knownFolder;
    }
  }

  public IReadOnlyDictionary<Guid, IKnownFolder> FolderMap { get =>  _folderMap; }

  /// <summary>
  /// Find a folder by ID in this instance
  /// </summary>
  public IKnownFolder? Find(Guid folderId)
  {
    return _folderMap.TryGetValue(folderId, out var knownFolder) ? knownFolder : null;
  }

  public string ExpandFolder(string mixedPath)
  {
    var match = Regex.Match(
      mixedPath,
      @"^\{([0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12})\}([^*?:]+)$",
      RegexOptions.IgnoreCase);
    if(match.Success
      && Guid.TryParse(match.Groups[1].Value, out var guid)
      && _folderMap.TryGetValue(guid, out var knownFolder))
    {
      if(!String.IsNullOrEmpty(knownFolder.Path))
      {
        return knownFolder.Path + match.Groups[2].Value;
      }
    }
    return mixedPath;
  }

  /// <summary>
  /// Find a folder by ID in the singleton instance
  /// </summary>
  public static IKnownFolder? FindById(Guid folderId)
  {
    return Instance.Find(folderId);
  }

  /// <summary>
  /// If <paramref name="mixedPath"/> starts with a known folder GUID, expand it.
  /// Otherwise return the input unmodified.
  /// </summary>
  public static string ExpandKnownFolder(string mixedPath)
  {
    return Instance.ExpandFolder(mixedPath);
  }

  /// <summary>
  /// Singleton instance
  /// </summary>
  public static KnownFolderMap Instance { get; } = new KnownFolderMap();

}
