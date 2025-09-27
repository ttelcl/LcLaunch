/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Ttelcl.Persistence.API;

namespace LcLauncher.IconTools;

/// <summary>
/// How hard to look for an icon in <see cref="IconCache"/>
/// </summary>
public enum IconCacheLoadLevel
{

  /// <summary>
  /// Only get the icon if it was already in the cache. If not in the
  /// cache, or the cache entry is null, return null.
  /// </summary>
  FromCache,

  /// <summary>
  /// Return the icon if it is in the cache. If it was not in the cache
  /// (neither as success or as failed load), try to load it from the
  /// backing store. If a load attempt previously failed this will return
  /// null without retrying.
  /// </summary>
  LoadIfMissing,

  /// <summary>
  /// Return the icon from the cache if it is there and not null.
  /// Otherwise try to load it from the backing store (that includes
  /// retrying a previously failed load).
  /// </summary>
  LoadAlways
}

/// <summary>
/// Wraps a blob store and caches the icons loaded from and it and
/// put in it.
/// </summary>
public class IconCache
{
  private readonly Dictionary<HashId, BitmapSource?> _icons;

  public IconCache(
    IBlobBucket blobBucket)
  {
    BlobBucket = blobBucket;
    _icons = [];
  }

  public IBlobBucket BlobBucket { get; }

  /// <summary>
  /// Preload the specified icons into this cache
  /// </summary>
  /// <param name="ids"></param>
  public void Preload(IEnumerable<HashId> ids)
  {
    using var reader = StartReader();
    foreach(var id in ids)
    {
      reader.FindIcon(id, IconCacheLoadLevel.LoadIfMissing);
    }
  }

  /// <summary>
  /// Open a short-lived object for retrieving icons from this cache,
  /// possibly loading them from disk. 
  /// The returned reader avoids opening the underlying blob store
  /// if possible. Multiple readers can NOT coexist, nor can they
  /// coexist with writers. So, remember to Dispose the returned reader.
  /// </summary>
  /// <returns></returns>
  public Reader StartReader()
  {
    return new Reader(this);
  }

  /// <summary>
  /// Open a short-lived object for writing icons to this cache and
  /// the backing store. <see cref="StartReader"/> cannot be used
  /// until this writer is disposed.
  /// </summary>
  /// <returns></returns>
  public Writer StartWriter()
  {
    return new Writer(this);
  }

  /// <summary>
  /// Functionality to get icons from this cache and load entries from
  /// disk if needed. This class postpones opening the backing cache
  /// implementation as long as possible.
  /// </summary>
  public class Reader: IDisposable
  {
    private IBlobBucketReader? _blobReader = null;
    private bool _disposed;

    internal Reader(IconCache cache)
    {
      Cache = cache;
    }

    /// <summary>
    /// The cache owning this reader
    /// </summary>
    public IconCache Cache { get; }

    /// <summary>
    /// Look up the icon in the cache, falling back to the backing cache
    /// if necessary, as determined by <paramref name="loadLevel"/>.
    /// </summary>
    /// <param name="hashId">
    /// The ID of the icon to load
    /// </param>
    /// <param name="loadLevel">
    /// Determines what to do if the icon is not in the cache at all or
    /// if it is null there (indicating a previous failed load).
    /// </param>
    /// <returns></returns>
    public BitmapSource? FindIcon(HashId? hashId, IconCacheLoadLevel loadLevel)
    {
      if(hashId == null)
      {
        return null;
      }
      ObjectDisposedException.ThrowIf(_disposed, this);
      if(Cache._icons.TryGetValue(hashId.Value, out var icon))
      {
        if(icon != null)
        {
          return icon;
        }
        if(loadLevel == IconCacheLoadLevel.LoadAlways)
        {
          Cache._icons.Remove(hashId.Value); // mark as not checked and fall through
        }
        else
        {
          return null;
        }
      }
      if(loadLevel == IconCacheLoadLevel.FromCache)
      {
        return null;
      }
      var bucket = Cache.BlobBucket;
      if(bucket.ContainsKey(hashId.Value))
      {
        if(_blobReader == null)
        {
          // We can no longer postpone opening the underlying blob store
          _blobReader = Cache.BlobBucket.OpenReader();
        }
        icon = _blobReader.TryReadIcon(hashId.Value);
      }
      Cache._icons[hashId.Value] = icon; // even if null!
      return icon;
    }

    /// <summary>
    /// Close this <see cref="IconCache.Reader"/>
    /// </summary>
    public void Dispose()
    {
      if(!_disposed)
      {
        _disposed = true;
        _blobReader?.Dispose();
        _blobReader = null;
        GC.SuppressFinalize(this);
      }
    }
  }

  /// <summary>
  /// Functionality to put icons in this cache and the backing store.
  /// Unlike <see cref="Reader"/> this class makes no attempt to postpone
  /// opening the backing store writer (since there are no good use cases
  /// to warrant that effort).
  /// </summary>
  public class Writer: IDisposable
  {
    private IBlobBucketWriter? _writer;
    private bool _disposed;

    internal Writer(IconCache cache)
    {
      Cache = cache;
      _writer = Cache.BlobBucket.OpenWriter();
    }

    public IconCache Cache { get; }

    /// <summary>
    /// Put an icon in the cache and return its ID. Duplicate
    /// icons are skipped.
    /// </summary>
    /// <param name="icon"></param>
    /// <returns></returns>
    public HashId PutIcon(BitmapSource icon)
    {
      ObjectDisposedException.ThrowIf(_writer==null, this);
      var hashId = _writer.TryWriteIcon(icon);
      Cache._icons[hashId] = icon;
      return hashId;
    }

    public void Dispose()
    {
      if(!_disposed)
      {
        var writer = _writer;
        _writer = null;
        _disposed = true;
        writer?.Dispose();
        GC.SuppressFinalize(this);
      }
    }
  }
}
