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
using LcLauncher.Storage.BlobsStorage;

using Microsoft.WindowsAPICodePack.Shell;

using Newtonsoft.Json;

namespace LcLauncher.Models;

/// <summary>
/// An ordered list of tiles, together with an ID.
/// This is not serialized direcly, but wraps the
/// result or source of serialization.
/// </summary>
public class TileList
{
  private BlobStorage _iconCache;

  /// <summary>
  /// Create a new TileList. Use <see cref="Load"/> or
  /// <see cref="Create(Guid?)"/> to call this constructor.
  /// </summary>
  private TileList(
    Guid id,
    IEnumerable<TileData?> tiles,
    LcLaunchDataStore store)
  {
    Id = id;
    Tiles = tiles.ToList();
    Store = store;
    _iconCache = store.GetBlobs(Id.ToString() + ".icon-cache", false);
  }

  /// <summary>
  /// Create a brand new empty TileList, optionally using
  /// a predefined ID.
  /// </summary>
  /// <param name="id">
  /// The optional ID to use. If null, a new ID is generated.
  /// </param>
  /// <returns></returns>
  public static TileList Create(
    LcLaunchDataStore store,
    Guid? id = null)
  {
    return new TileList(id ?? Guid.NewGuid(), [], store);
  }

  /// <summary>
  /// Load a TileList from a *.tile-list file in the given folder.
  /// </summary>
  /// <param name="store">
  /// The store in which to look for the file.
  /// </param>
  /// <param name="id">
  /// The ID, implying the file name.
  /// </param>
  /// <returns>
  /// Returns the list that was loaded, or null if the file
  /// did not exist.
  /// </returns>
  public static TileList? Load(
    LcLaunchDataStore store,
    Guid id)
  {
    var list = store.LoadData<TileData?[]>(
      id, ".tile-list");
    return list == null ? null : new TileList(id, list, store);
  }

  public void Save()
  {
    Store.SaveData(Id.ToString(), ".tile-list", Tiles);
  }

  public BlobStorage GetIconCache(
    bool reInitialize = false)
  {
    _iconCache.Initialize(reInitialize);
    return _iconCache;
  }

  public Guid Id { get; }

  public List<TileData?> Tiles { get; }

  /// <summary>
  /// The store in which this list is saved and its icons are cached.
  /// </summary>
  public LcLaunchDataStore Store { get; }

  public bool CacheIcon(BitmapSource icon, out BlobEntry blobEntry)
  {
    var iconCache = GetIconCache();
    var encoder = new PngBitmapEncoder();
    using var stream = new MemoryStream(0x4000);
    encoder.Frames.Add(BitmapFrame.Create(icon));
    encoder.Save(stream);
    var iconBytes = stream.ToArray();
    var added = iconCache.AppendOrRetrieveBlob(
      iconBytes, out blobEntry);
    return added;
  }

  public BitmapSource? LoadCachedIcon(BlobEntry entry)
  {
    var iconCache = GetIconCache();
    try
    {
      using var iconStream = iconCache.OpenBlobStream(entry);
      return BitmapFrame.Create(iconStream);
    }
    catch(Exception ex)
    {
      Trace.TraceError(
        $"Failed to decode icon {entry.Hash} from cache for tile list {Id}: {ex}");
      return null;
    }
  }

  public BitmapSource? LoadCachedIcon(string hashPrefix)
  {
    var entry = GetIconCache()[hashPrefix];
    return entry == null ? null : LoadCachedIcon(entry);
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
