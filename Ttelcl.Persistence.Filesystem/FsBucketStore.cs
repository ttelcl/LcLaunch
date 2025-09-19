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

using Ttelcl.Persistence.API;

namespace Ttelcl.Persistence.Filesystem;

/// <summary>
/// Implements <see cref="IBucketStore"/> and its specializations using
/// files and folders directly
/// </summary>
public class FsBucketStore: IBucketStore, IJsonBucketStore, IBlobBucketStore
{
  private readonly Dictionary<string, IBucketBase> _bucketRegistry;

  /// <summary>
  /// Create a new FsBucketStore
  /// </summary>
  public FsBucketStore(
    string storeFolder)
  {
    _bucketRegistry = new Dictionary<string, IBucketBase>(
      StringComparer.OrdinalIgnoreCase);
    StoreFolder = Path.GetFullPath(storeFolder);
    if(!Directory.Exists(StoreFolder))
    {
      Directory.CreateDirectory(StoreFolder);
    }
  }

  /// <summary>
  /// The folder where the store lives
  /// </summary>
  public string StoreFolder { get; }

  /// <inheritdoc/>
  public IBucketBase? GetBucket(string bucketName)
  {
    if(!NamingRules.IsValidBucketName(bucketName))
    {
      throw new ArgumentOutOfRangeException(
        nameof(bucketName), 
        $"Not a valid bucket name: '{bucketName}'");
    }
    return _bucketRegistry.TryGetValue(bucketName, out var baseBucket)
      ? baseBucket
      : null;
  }

  /// <inheritdoc/>
  public IJsonBucket<T>? GetJsonBucket<T>(
    string bucketName, bool create = false)
    where T : class
  {
    var bucket = this.GetBucket<T>(bucketName);
    if(bucket == null)
    {
      if(create)
      {
        var b = new FsJsonBucket<T>(this, bucketName);
        _bucketRegistry.Add(bucketName, b);
        return b;
      }
      return null;
    }
    if(bucket is not IJsonBucket<T> typedBucket)
    {
      throw new InvalidOperationException(
        $"Unexpected implementation type for bucket '{bucketName}' (not a JSON bucket)");
    }
    return typedBucket;
  }

  /// <inheritdoc/>
  public IBlobBucket? GetBlobBucket(
    string bucketName, bool create = false)
  {
    var bucket = this.GetBucket<byte[]>(bucketName);
    if(bucket == null)
    {
      if(create)
      {
        var b = new FsBlobBucket(this, bucketName);
        _bucketRegistry.Add(bucketName, b);
        return b;
      }
      return null;
    }
    if(bucket is not IBlobBucket typedBucket)
    {
      throw new InvalidOperationException(
        $"Unexpected implementation type for bucket '{bucketName}' (not a BLOB bucket)");
    }
    return typedBucket;
  }
}