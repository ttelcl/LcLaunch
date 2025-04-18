﻿/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LcLauncher.Storage.BlobsStorage;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;
using System.Windows.Media.Imaging;
using LcLauncher.Models;

namespace LcLauncher.Storage;

/// <summary>
/// Provides a higher level interface to icon caches
/// </summary>
public class IconCache
{
  /// <summary>
  /// Do not access directly, only through <see cref="GetIconStorage"/>.
  /// </summary>
  private readonly BlobStorage _iconCache;

  /// <summary>
  /// Create a new IconCache
  /// </summary>
  /// <param name="store">
  /// The underlying data store. This determines the folder
  /// where the cache files are stored.
  /// </param>
  /// <param name="cacheId">
  /// The id of this icon cache (normally the same as the
  /// id of the tile list it relates to). This determines
  /// the cache file names.
  /// </param>
  public IconCache(
    JsonDataStore store,
    Guid cacheId)
  {
    CacheId = cacheId;
    // Do not pre-initialize the storage; only initialize it
    // when it is actually used.
    _iconCache = store.GetBlobs(cacheId.ToString() + ".icon-cache", false);
  }

  public Guid CacheId { get; }

  /// <summary>
  /// Try to find the icon by ID in this cache.
  /// </summary>
  /// <param name="hashPrefix">
  /// The hash of the icon to load. A unique prefix
  /// is sufficient.
  /// </param>
  /// <returns>
  /// The icon, if found.
  /// </returns>
  public BitmapSource? LoadCachedIcon(string hashPrefix)
  {
    var entry = GetIconStorage()[hashPrefix];
    return entry == null ? null : LoadCachedIconEntry(entry);
  }

  /// <summary>
  /// Create an icon, put it in the cache, and return the hash.
  /// Expect this to be slow.
  /// </summary>
  /// <param name="iconSource">
  /// The file for which to retrieve the icon.
  /// </param>
  /// <param name="size">
  /// The icon size to retrieve. Supported sizes are 16, 32, 48, 256.
  /// </param>
  /// <param name="icon">
  /// Returns the icon that was loaded, if found.
  /// </param>
  /// <returns>
  /// The hash of the icon in the cache, or null if the icon could not be loaded.
  /// </returns>
  public string? CacheIcon(
    string iconSource, int size, out BitmapSource? icon)
  {
    icon = IconCache.IconForFile(iconSource, size);
    if(icon == null)
    {
      return null;
    }
    CacheIcon(icon, out var blobEntry);
    return blobEntry.Hash;
  }

  public IconHashes? CacheIcons(
    string iconSource, IconSize sizes)
  {
    var icons = IconCache.IconsForFile(iconSource, sizes);
    if(icons == null)
    {
      return null;
    }
    string? hash16 = null;
    string? hash32 = null;
    string? hash48 = null;
    string? hash256 = null;
    var icon16 = icons[0];
    var icon32 = icons[1];
    var icon48 = icons[2];
    var icon256 = icons[3];
    if(icon16 != null)
    {
      CacheIcon(icon16, out var be);
      hash16 = be.Hash;
    }
    if(icon32 != null)
    {
      CacheIcon(icon32, out var be);
      hash32 = be.Hash;
    }
    if(icon48 != null)
    {
      CacheIcon(icon48, out var be);
      hash48 = be.Hash;
    }
    if(icon256 != null)
    {
      CacheIcon(icon256, out var be);
      hash256 = be.Hash;
    }
    if(hash16 == null && hash32 == null && hash48 == null && hash256 == null)
    {
      return null;
    }
    return new IconHashes() {
      Small = hash16,
      Medium = hash32,
      Large = hash48,
      ExtraLarge = hash256,
    };
  }

  public void Initialize(
    bool reInitialize = false)
  {
    GetIconStorage(reInitialize);
  }

  private bool CacheIcon(BitmapSource icon, out BlobEntry blobEntry)
  {
    var iconCache = GetIconStorage();
    var encoder = new PngBitmapEncoder();
    using var stream = new MemoryStream(0x4000);
    encoder.Frames.Add(BitmapFrame.Create(icon));
    encoder.Save(stream);
    var iconBytes = stream.ToArray();
    var added = iconCache.AppendOrRetrieveBlob(
      iconBytes, out blobEntry);
    return added;
  }

  private BitmapSource? LoadCachedIconEntry(BlobEntry entry)
  {
    var iconCache = GetIconStorage();
    try
    {
      using var iconStream = iconCache.OpenBlobStream(entry);
      return BitmapFrame.Create(
        iconStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
    }
    catch(Exception ex)
    {
      Trace.TraceError(
        $"Failed to decode icon {entry.Hash} from cache: {ex}");
      return null;
    }
  }

  private BlobStorage GetIconStorage(
    bool reInitialize = false)
  {
    _iconCache.Initialize(reInitialize);
    return _iconCache;
  }

  /// <summary>
  /// Try to get the icon for a file, using the shell.
  /// </summary>
  /// <param name="path">
  /// The full path to the file.
  /// </param>
  /// <param name="size">
  /// The size of the icon to get. Supported sizes are 16, 32, 48, 256.
  /// The actual returned size may vary.
  /// </param>
  /// <returns></returns>
  private static BitmapSource? IconForFile(string path, int size)
  {
    try
    {
      using var iconShell = ShellObject.FromParsingName(path);
      var thumbnail = iconShell.Thumbnail;
      thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
      return size switch {
        16 => thumbnail.SmallBitmapSource,
        32 => thumbnail.MediumBitmapSource,
        48 => thumbnail.LargeBitmapSource,
        256 => thumbnail.ExtraLargeBitmapSource,
        _ => null,
      };
    }
    catch(ShellException ex)
    {
      Trace.TraceError(
        $"IconForFile: Error probing icon file {path}: {ex}");
      return null;
    }
  }

  private static BitmapSource?[]? IconsForFile(
    string path, IconSize sizes)
  {
    try
    {
      var icons = new BitmapSource?[4];
      using var iconShell = ShellObject.FromParsingName(path);
      var thumbnail = iconShell.Thumbnail;
      thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
      if(sizes.HasFlag(IconSize.Small))
      {
        icons[0] = thumbnail.SmallBitmapSource;
      }
      if(sizes.HasFlag(IconSize.Medium))
      {
        icons[1] = thumbnail.MediumBitmapSource;
      }
      if(sizes.HasFlag(IconSize.Large))
      {
        icons[2] = thumbnail.LargeBitmapSource;
      }
      if(sizes.HasFlag(IconSize.ExtraLarge))
      {
        icons[3] = thumbnail.ExtraLargeBitmapSource;
      }
      return icons;
    }
    catch(ShellException ex)
    {
      Trace.TraceError(
        $"IconForFile: Error probing icon file {path}: {ex}");
      return null;
    }
  }

}
