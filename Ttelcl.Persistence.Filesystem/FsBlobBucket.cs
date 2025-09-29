/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ttelcl.Persistence.API;

namespace Ttelcl.Persistence.Filesystem;

/// <summary>
/// Implements <see cref="IBlobBucket"/>
/// using a "blobs file" plus "blobs index file" in the store folder.
/// Each bucket is implemented as a single file pair. Note that this
/// implementation does not support deleting blobs.
/// </summary>
public class FsBlobBucket: IBlobBucket, IHasFolder
{
  private BlobBucketReader? _currentReader;
  private BlobBucketWriter? _currentWriter;

  /// <summary>
  /// Create a new FsBlobBucket
  /// </summary>
  internal FsBlobBucket(
    FsBucketStore store,
    string bucketName)
  {
    if(!NamingRules.IsValidBucketName(bucketName))
    {
      throw new ArgumentException(
        $"Not a valid bucket name: '{bucketName}'");
    }
    BucketName = bucketName;
    Store = store;
    var blobsFileName = "blobs." + bucketName + ".blobs";
    var fullBlobsFileName = Path.Combine(store.StoreFolder, blobsFileName);
    BlobsFile = new FsBlobsFile(fullBlobsFileName);
    BlobIndexFile = new FsBlobIndexFile(BlobsFile);
  }

  /// <summary>
  /// The store this bucket is part of
  /// </summary>
  public FsBucketStore Store { get; }

  /// <inheritdoc/>
  public string StorageFolder => Store.StorageFolder;

  /// <summary>
  /// The name of the bucket
  /// </summary>
  public string BucketName { get; }

  /// <summary>
  /// The *.blobs file storing this bucket's blobs
  /// </summary>
  public FsBlobsFile BlobsFile { get; }

  /// <summary>
  /// The *.blob-idx file for <see cref="BlobsFile"/>
  /// </summary>
  public FsBlobIndexFile BlobIndexFile { get; }

  /// <summary>
  /// The type of the items stored in this bucket: byte[].
  /// </summary>
  public Type BucketType => typeof(byte[]);

  /// <inheritdoc/>
  public bool ContainsKey(HashId id)
  {
    return BlobIndexFile.Descriptors.ContainsKey(id);
  }

  /// <inheritdoc/>
  public IEnumerable<HashId> Keys()
  {
    return BlobIndexFile.Descriptors.Keys;
  }

  /// <inheritdoc/>
  public void Erase()
  {
    BlobIndexFile.Erase();
    BlobsFile.Erase();
  }

  /// <inheritdoc/>
  public bool TryGetEntry(
    HashId id, [NotNullWhen(true)] out IBlobEntry? entry)
  {
    if(BlobIndexFile.Descriptors.TryGetValue(id, out var e))
    {
      entry = e;
      return true;
    }
    else
    {
      entry = null;
      return false;
    }
  }

  /// <inheritdoc/>
  public IBlobBucketReader OpenReader()
  {
    if(_currentReader != null)
    {
      throw new InvalidOperationException(
        "Cannot open a blob reader while another one is still active");
    }
    if(_currentWriter != null)
    {
      throw new InvalidOperationException(
        "Cannot open a blob reader while a blob writer is still active");
    }
    var reader = new BlobBucketReader(this);
    _currentReader = reader;
    return reader;
  }

  /// <inheritdoc/>
  public IBlobBucketWriter OpenWriter()
  {
    if(_currentWriter != null)
    {
      throw new InvalidOperationException(
        "Cannot open a blob writer while another one is still active");
    }
    if(_currentReader != null)
    {
      throw new InvalidOperationException(
        "Cannot open a blob writer while a blob reader is still active");
    }
    var writer = new BlobBucketWriter(this);
    _currentWriter = writer;
    return writer;
  }

  private class BlobBucketReader: IBlobBucketReader, IHasFolder
  {
    private FileStream? _blobReadStream = null;

    public BlobBucketReader(FsBlobBucket owner)
    {
      Owner = owner;
      _blobReadStream = owner.BlobsFile.OpenRead();
    }

    public FsBlobBucket Owner { get; }

    /// <inheritdoc/>
    public string StorageFolder => Owner.StorageFolder;

    public bool TryGetBlob(HashId id, [NotNullWhen(true)] out byte[]? blob)
    {
      ObjectDisposedException.ThrowIf(_blobReadStream == null, this);
      if(Owner.BlobIndexFile.Descriptors.TryGetValue(id, out var descriptor))
      {
        blob = Owner.BlobsFile.ReadBlob(_blobReadStream, descriptor);
        return true;
      }
      else
      {
        blob = null;
        return false;
      }
    }

    public void Dispose()
    {
      if(_blobReadStream != null)
      {
        var fs = _blobReadStream;
        _blobReadStream = null;
        if(Owner._currentReader == this)
        {
          Owner._currentReader = null;
        }
        fs.Dispose();
      }
    }
  }

  private class BlobBucketWriter: IBlobBucketWriter, IHasFolder
  {
    private FileStream? _blobAppendStream = null;
    private FileStream? _indexAppendStream = null;

    public BlobBucketWriter(FsBlobBucket owner)
    {
      Owner = owner;
      _blobAppendStream = Owner.BlobsFile.OpenAppend();
      _indexAppendStream = Owner.BlobIndexFile.OpenAppend();
    }

    public FsBlobBucket Owner { get; }

    /// <inheritdoc/>
    public string StorageFolder => Owner.StorageFolder;

    public bool TryPutBlob(byte[] blob, out HashId id)
    {
      ObjectDisposedException.ThrowIf(
        _blobAppendStream == null || _indexAppendStream == null,
        this);
      var added = Owner.BlobIndexFile.TryAppendBlob(
        blob,
        _blobAppendStream,
        _indexAppendStream,
        out var entry);
      id = entry.Id;
      return added;
    }

    public void Dispose()
    {
      var bfs = _blobAppendStream;
      var ifs = _indexAppendStream;
      _blobAppendStream = null;
      _indexAppendStream = null;
      if(Owner._currentWriter == this)
      {
        Owner._currentWriter = null;
      }
      if(bfs != null)
      {
        bfs.Dispose();
      }
      if(ifs != null)
      {
        ifs.Dispose();
      }
    }
  }
}
