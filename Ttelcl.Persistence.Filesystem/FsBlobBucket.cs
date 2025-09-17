/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ttelcl.Persistence.API;

namespace Ttelcl.Persistence.Filesystem;

/// <summary>
/// Description of FsBlobBucket
/// </summary>
public class FsBlobBucket: IBlobBucket, ICasBlobBucket
{
  /// <summary>
  /// Create a new FsBlobBucket
  /// </summary>
  internal FsBlobBucket(
    string bucketName)
  {
    BucketName = bucketName;
  }

  /// <summary>
  /// The name of the bucket
  /// </summary>
  public string BucketName { get; }

  /// <summary>
  /// The type of the items stored in this bucket: byte[].
  /// </summary>
  public Type BucketType => typeof(byte[]);

  /// <inheritdoc/>
  public byte[]? this[HashId id] {
    get {
      throw new NotImplementedException();
    }
    set {
      throw new NotImplementedException();
    }
  }

  /// <inheritdoc/>
  public bool ContainsKey(HashId id)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc/>
  public IEnumerable<TickId> Keys()
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc/>
  public bool TryFindBlob(
    HashId id,
    [NotNullWhen(true)] out byte[]? blob)
  {
    blob = this[id];
    return blob != null;
  }

  /// <inheritdoc/>
  public bool TryPutBlob(
    byte[] blob,
    out HashId id)
  {
    id = HashId.FromBlob(blob);
    if(ContainsKey(id))
    {
      return false;
    }
    this[id] = blob;
    return true;
  }
}