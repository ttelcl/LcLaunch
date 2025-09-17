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
public interface IBlobBucketStore
{
  /// <summary>
  /// Get a blob bucket by bucket name. If not found,
  /// the behaviour depends on <paramref name="create"/>: if true,
  /// a bucket will be created, if false, null is returned.
  /// </summary>
  /// <param name="bucketName"></param>
  /// <param name="create"></param>
  /// <returns></returns>
  IBlobBucket GetBucket(string bucketName, bool create);
}

/// <summary>
/// The buckets in a <see cref="IBlobBucketStore"/>.
/// <see cref="IBucketBase.BucketType"/> will be byte[].
/// </summary>
public interface IBlobBucket: IBucketBase
{
  /// <summary>
  /// Get, Put, or delete a blob by its ID. Note
  /// that for CAS Blob storage, the ID must be calculated
  /// from the blob upon storing.
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  byte[]? this[HashId id] { get; set; }

  /// <summary>
  /// Test if the key is present in the bucket, without actually
  /// retrieving the blob
  /// </summary>
  public bool ContainsKey(HashId id);
}

/// <summary>
/// A layer on top of <see cref="IBlobBucket"/> implementing
/// Content Addressable Store functionality
/// </summary>
public interface ICasBlobBucket: IBlobBucket
{
  /// <summary>
  /// Look up a blob by ID. On success, true is returned and
  /// <paramref name="blob"/> is set to the blob found.
  /// On failure, false is returned and <paramref name="blob"/>
  /// is set to null
  /// </summary>
  /// <param name="id"></param>
  /// <param name="blob"></param>
  /// <returns></returns>
  bool TryFindBlob(HashId id, [NotNullWhen(true)] out byte[]? blob);

  /// <summary>
  /// Put a blob in the store and generate its ID. If the blob
  /// was already present it is not replaced (it is assumed the
  /// blob has the exact same content)
  /// </summary>
  /// <param name="blob">
  /// The blob to store
  /// </param>
  /// <param name="id">
  /// Upon return: the ID calculated from the blob
  /// </param>
  /// <returns>
  /// True if the blob was stored, false if it was present already.
  /// </returns>
  bool TryPutBlob(byte[] blob, out HashId id);

}
