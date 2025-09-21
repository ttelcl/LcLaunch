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

using Microsoft.WindowsAPICodePack.Shell;

using LcLauncher.Persistence;
using LcLauncher.Storage.BlobsStorage;
using LcLauncher.Models;
using LcLauncher.ShellApps;

using LcLauncher.DataModel.Utilities;

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

  public byte[]? LoadCachedBlob(string hashPrefix)
  {
    // for use while converting from V2 to V3 model
    var entry = GetIconStorage()[hashPrefix];
    if(entry != null)
    {
      var iconCache = GetIconStorage();
      return iconCache.ReadBlob(entry);
    }
    return null;
  }

  public IconHashes? CacheIcons(
    string iconSource, IconSize sizes)
  {
    var icons = IconCache.IconsForSource(iconSource, sizes);
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
      var added = CacheIcon(icon16, out var be) ? "ADDED" : "existing";
      Trace.TraceInformation(
        $"Icon16 {added}: {be.Hash}");
      hash16 = be.Hash;
    }
    if(icon32 != null)
    {
      var added = CacheIcon(icon32, out var be) ? "ADDED" : "existing";
      Trace.TraceInformation(
        $"Icon32 {added}: {be.Hash}");
      hash32 = be.Hash;
    }
    if(icon48 != null)
    {
      var added = CacheIcon(icon48, out var be) ? "ADDED" : "existing";
      Trace.TraceInformation(
        $"Icon48 {added}: {be.Hash}");
      hash48 = be.Hash;
    }
    if(icon256 != null)
    {
      var added = CacheIcon(icon256, out var be) ? "ADDED" : "existing";
      Trace.TraceInformation(
        $"Icon256 {added}: {be.Hash}");
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
  /// Load icons for the given source from the shell.
  /// </summary>
  /// <param name="source">
  /// The icon source: a file name (document or executable),
  /// or an application ID prefixed with "shell:AppsFolder\".
  /// </param>
  /// <param name="sizes"></param>
  /// <returns></returns>
  public static BitmapSource?[]? IconsForSource(
    string source, IconSize sizes)
  {
    bool useAppIcon = ShellAppTools.HasShellAppsFolderPrefix(source);
    try
    {
      var icons = new BitmapSource?[4];
      using var iconShell = ShellObject.FromParsingName(source);
      var thumbnail = iconShell.Thumbnail;
      if(useAppIcon)
      {
        thumbnail.FormatOption = ShellThumbnailFormatOption.Default;
        // Beware: 'Default' causes incorrect icon sizes, do not
        // rely on SmallBBitmapSource etc properties in this case,
        // use CurrentSize + BitmapSource instead.
        if(sizes.HasFlag(IconSize.Small))
        {
          thumbnail.CurrentSize = DefaultIconSize.Small;
          icons[0] = thumbnail.BitmapSource;
        }
        if(sizes.HasFlag(IconSize.Medium))
        {
          thumbnail.CurrentSize = DefaultIconSize.Medium;
          icons[1] = thumbnail.BitmapSource;
        }
        if(sizes.HasFlag(IconSize.Large))
        {
          thumbnail.CurrentSize = DefaultIconSize.Large;
          icons[2] = thumbnail.BitmapSource;
        }
        if(sizes.HasFlag(IconSize.ExtraLarge))
        {
          thumbnail.CurrentSize = DefaultIconSize.ExtraLarge;
          icons[3] = thumbnail.BitmapSource;
        }
        return icons;
      }
      else
      {
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
    }
    catch(ShellException ex)
    {
      Trace.TraceError(
        $"IconForFile: Error probing icon source {source}: {ex}");
      return null;
    }
  }

}
