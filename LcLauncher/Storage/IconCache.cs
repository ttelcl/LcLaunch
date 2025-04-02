/*
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
    LcLaunchDataStore store,
    Guid cacheId)
  {
    Store = store;
    CacheId = cacheId;
    // Do not pre-initialize the storage; only initialize it
    // when it is actually used.
    _iconCache = store.GetBlobs(cacheId.ToString() + ".icon-cache", false);
  }

  public LcLaunchDataStore Store { get; }

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

  internal BitmapSource? LoadCachedIconEntry(BlobEntry entry)
  {
    var iconCache = GetIconStorage();
    try
    {
      using var iconStream = iconCache.OpenBlobStream(entry);
      return BitmapFrame.Create(iconStream);
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
  public static BitmapSource? IconForFile(string path, int size)
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

}
