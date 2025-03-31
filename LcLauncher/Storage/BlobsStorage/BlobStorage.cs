/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace LcLauncher.Storage.BlobsStorage;

/// <summary>
/// A storage for blobs of data. Implemented as a pair of
/// a files, once for storing the actual blobs
/// and one as an index of the blobs. Blobs are identified
/// by their SHA1 hash, in Content Addressable Storage style,
/// like Git.
/// </summary>
public class BlobStorage
{
  /// <summary>
  /// Create a new BlobStorage
  /// </summary>
  /// <param name="blobsFileName">
  /// The name of the blobs half of the storage pair
  /// (with *.blobs file extension)
  /// </param>
  /// <param name="initialize">
  /// If true, the storage is initialized, i.e. the blobs file
  /// is created if it did not exist, and the index is updated.
  /// </param>
  public BlobStorage(
    string blobsFileName,
    bool initialize)
  {
    if(!blobsFileName.EndsWith(".blobs"))
    {
      throw new ArgumentException(
        "Blobs file name must end with '.blobs'");
    }
    BlobsFileName = Path.GetFullPath(blobsFileName);
    IndexFileName = Path.ChangeExtension(BlobsFileName, ".blob-idx");
    BlobContainerFile = new BlobsFile(this);
    IndexFile = new BlobIndexFile(this);
    if(initialize)
    {
      Initialize(true);
    }
  }

  public string BlobsFileName { get; }

  public string IndexFileName { get; }

  public BlobsFile BlobContainerFile { get; }

  public BlobIndexFile IndexFile { get; }

  public bool IsInitialized { get; private set; }

  public void Delete()
  {
    if(File.Exists(BlobsFileName))
    {
      File.Delete(BlobsFileName);
    }
    IndexFile.Update();
    if(File.Exists(IndexFileName))
    {
      Trace.TraceError(
        $"Expecting index file NOT to exist after Update() after Delete()");
    }
  }

  public void Initialize(bool force)
  {
    if(!IsInitialized || force)
    {
      BlobContainerFile.OpenAppend().Dispose();
      IndexFile.Update();
    }
    IsInitialized = true;
  }

  /// <summary>
  /// Append a blob to the storage, or retrieve an existing blob
  /// </summary>
  /// <param name="blob">
  /// The blob to add. If the blob already exists in the storage
  /// (as determined by its hash), it is not added again.
  /// </param>
  /// <param name="entry">
  /// The Blob Index Entry for the blob that was added or retrieved.
  /// </param>
  /// <returns>
  /// True if the blob was added, false if it already existed.
  /// </returns>
  public bool AppendOrRetrieveBlob(
    byte[] blob, out BlobEntry entry)
  {
    Span<byte> hashBuffer = stackalloc byte[20];
    SHA1.HashData(blob, hashBuffer);
    var hashString = BlobEntry.HashString(hashBuffer);
    var existing = IndexFile.FindOneByPrefix(hashString);
    if(existing != null)
    {
      // Blob already exists - avoid adding a duplicate!
      Trace.TraceInformation(
        $"Blob already exists, not adding it again: {hashString}");
      entry = existing;
      return false;
    }
    entry = BlobContainerFile.AppendBlob(blob, hashBuffer);
    IndexFile.Update();
    return true;
  }

  /// <summary>
  /// Returns the first blob entry with the given prefix.
  /// </summary>
  public BlobEntry? this[string prefix] {
    get => IndexFile.FindAllByPrefix(prefix).FirstOrDefault();
  }

  /// <summary>
  /// Get the data for the given blob entry as a stream.
  /// The current implementation returns a MemoryStream.
  /// </summary>
  /// <param name="entry"></param>
  /// <returns></returns>
  public Stream OpenBlobStream(
    BlobEntry entry)
  {
    // for now, use a MemoryStream because we don't have SubStreams yet
    var blob = BlobContainerFile.ReadBlob(entry);
    return new MemoryStream(blob);
  }

  /// <summary>
  /// Get the blob data for the given entry.
  /// Alias for indexer this[BlobEntry].
  /// </summary>
  public byte[] ReadBlob(BlobEntry entry)
  {
    return BlobContainerFile.ReadBlob(entry);
  }

  /// <summary>
  /// Get the blob data for the given entry.
  /// Alias for <see cref="ReadBlob(BlobEntry)"/>
  /// </summary>
  /// <param name="entry"></param>
  /// <returns></returns>
  public byte[] this[BlobEntry entry] {
    get => ReadBlob(entry);
  }
}
