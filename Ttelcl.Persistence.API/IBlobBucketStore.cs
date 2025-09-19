/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ttelcl.Persistence.API;

/// <summary>
/// A storage API for hosting 'buckets' of blobs (byte arrays)
/// identified by a 64 bit id. This base API allows any ID,
/// but the intended primary use case is CAS mode, where
/// IDs are a hash of the stored blobs
/// </summary>
public interface IBlobBucketStore: IBucketStore
{
  /// <summary>
  /// Get a blob bucket by bucket name. If not found,
  /// the behaviour depends on <paramref name="create"/>: if true,
  /// a bucket will be created, if false, null is returned.
  /// </summary>
  /// <param name="bucketName"></param>
  /// <param name="create"></param>
  /// <returns></returns>
  IBlobBucket? GetBlobBucket(string bucketName, bool create);
}

/// <summary>
/// The buckets in a <see cref="IBlobBucketStore"/>.
/// <see cref="IBucketBase.BucketType"/> will be byte[].
/// </summary>
public interface IBlobBucket: IBucketBase<HashId>
{
  /// <summary>
  /// Look up the blob index entry for the given blob <paramref name="id"/>.
  /// This method is expected to be 'cheap'.
  /// </summary>
  /// <param name="id">
  /// The blob ID to look for
  /// </param>
  /// <param name="entry">
  /// The blob descriptor if successful, or null if not found.
  /// </param>
  /// <returns>
  /// True if found, false if not found
  /// </returns>
  bool TryGetEntry(HashId id, [NotNullWhen(true)] out IBlobEntry? entry);

  /// <summary>
  /// Open a short-lived API for reading blobs. Implementations may choose
  /// to disallow blob writes until this reader is disposed.
  /// </summary>
  /// <returns></returns>
  IBlobBucketReader OpenReader();

  /// <summary>
  /// Open a short lived API for writing blobs. Implementations may choose
  /// to disallow blob reads until this writer is closed. Implementations
  /// may choose to hide the written blobs from reading until this writer
  /// is closed.
  /// </summary>
  /// <returns></returns>
  IBlobBucketWriter OpenWriter();
}

/// <summary>
/// A short-lived read-only interface to read blobs. While this
/// is active, blob writing may be disabled
/// </summary>
public interface IBlobBucketReader: IDisposable
{

  /// <summary>
  /// Load the blob for the given blob <paramref name="id"/>.
  /// This may be an expensive operation
  /// </summary>
  /// <param name="id"></param>
  /// <param name="blob"></param>
  /// <returns></returns>
  bool TryGetBlob(HashId id, [NotNullWhen(true)] out byte[]? blob);
}

/// <summary>
/// A short-lived write-only API to add new blobs.
/// </summary>
public interface IBlobBucketWriter: IDisposable
{
  /// <summary>
  /// Add a blob and generate its ID. If the blob was already
  /// present nothing is written, and false is returned. Otherwise
  /// the blob is added and true is returned.
  /// </summary>
  /// <param name="blob"></param>
  /// <param name="id"></param>
  /// <returns></returns>
  bool TryPutBlob(byte[] blob, out HashId id);
}

/// <summary>
/// Describes an entry in the blob index
/// </summary>
public interface IBlobEntry
{
  /// <summary>
  /// The unique identifier of the blob, as derived from the blob's
  /// hash
  /// </summary>
  HashId Id { get; }

  /// <summary>
  /// The blob's size
  /// </summary>
  int Size { get; }
}

/// <summary>
/// Extension methods on blob bucket related interfaces
/// </summary>
public static class BlobBucketExtensions
{
  /// <summary>
  /// Put a blob in the blob bucket and return its ID. If the blob
  /// was already in the bucket it is not put again. This API does
  /// not tell you if the blob was put in newly, or if it was already
  /// there.
  /// </summary>
  /// <param name="writer">
  /// The writer opened with <see cref="IBlobBucket.OpenWriter"/>
  /// </param>
  /// <param name="blob">
  /// The blob to write
  /// </param>
  /// <returns>
  /// The <see cref="HashId"/> for the blob that can be used to retrieve
  /// the blob
  /// </returns>
  public static HashId PutBlob(
    this IBlobBucketWriter writer,
    byte[] blob)
  {
    writer.TryPutBlob(blob, out var id);
    return id;
  }

  /// <summary>
  /// Lookup the blob with the given <paramref name="id"/> in the
  /// bucket, returning it if found, or returning null if not found
  /// </summary>
  /// <param name="reader"></param>
  /// <param name="id"></param>
  /// <returns></returns>
  public static byte[]? GetBlob(
    this IBlobBucketReader reader,
    HashId id)
  {
    return
      reader.TryGetBlob(id, out var blob)
      ? blob
      : null;
  }
}


