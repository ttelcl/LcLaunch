/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using LcLauncher.Storage;

namespace LcLauncher.Persistence;

/// <summary>
/// File based implementation of <see cref="ILauncherIconCache"/>
/// </summary>
public class FileBasedIconCache: ILauncherIconCache
{
  /// <summary>
  /// Create a new FileBasedIconCache
  /// </summary>
  public FileBasedIconCache(
    IconCache host)
  {
    Host = host;
    CacheId = host.CacheId;
  }

  public IconCache Host { get; }

  public Guid CacheId { get; }

  public BitmapSource? LoadCachedIcon(
    string? hashPrefix)
  {
    if(string.IsNullOrEmpty(hashPrefix))
    {
      return null;
    }
    return Host.LoadCachedIcon(hashPrefix);
  }

  public IconHashes? CacheIcons(
    string iconSource,
    IconSize sizes = IconSize.Normal)
  {
    // For now, require iconSource to be an existing file.
    if(!File.Exists(iconSource))
    {
      return null;
    }
    var hashes = Host.CacheIcons(iconSource, sizes);
    return hashes;
  }
}