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
    IndexFileName = Path.ChangeExtension(BlobsFileName, ".blobs-index");
    BlobsFile = new BlobsFile(this);
    IndexFile = new BlobIndexFile(this);
    if(initialize)
    {
      Initialize();
    }
  }

  public string BlobsFileName { get; }

  public string IndexFileName { get; }

  public BlobsFile BlobsFile { get; }

  public BlobIndexFile IndexFile { get; }

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

  public void Initialize()
  {
    BlobsFile.OpenAppend().Dispose();
    IndexFile.Update();
  }

  public BlobEntry AppendOrRetrieveBlob(
    byte[] blob)
  {
    Span<byte> hashBuffer = stackalloc byte[20];
    SHA1.HashData(blob, hashBuffer);
    var hashString = BlobEntry.HashString(hashBuffer);
    var existing = IndexFile.FindOneByPrefix(hashString);
    if(existing != null)
    {
      // Blob already exists - avoid adding a duplicate!
      return existing;
    }
    var entry = BlobsFile.AppendBlob(blob, hashBuffer);
    IndexFile.Update();
    return entry;
  }
}
