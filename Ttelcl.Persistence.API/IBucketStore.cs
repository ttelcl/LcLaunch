/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ttelcl.Persistence.API;

/// <summary>
/// A generic bucket store, expected to implement 
/// <see cref="IJsonBucketStore"/> and / or <see cref="IBlobBucketStore"/>,
/// and possibly <see cref="ISingletonStore"/>, as refinements
/// </summary>
public interface IBucketStore
{
  /// <summary>
  /// Returns an existing bucket (previously created / allocated via
  /// <see cref="IJsonBucketStore.GetJsonBucket{T}(string, bool)"/>
  /// or <see cref="IBlobBucketStore.GetBlobBucket(string, bool)"/>)
  /// Provides a lower level service to build
  /// <see cref="IJsonBucketStore.GetJsonBucket{T}(string, bool)"/>
  /// and <see cref="IBlobBucketStore.GetBlobBucket(string, bool)"/>
  /// on top of.
  /// </summary>
  /// <param name="bucketName">
  /// The name of the bucket
  /// </param>
  /// <returns>
  /// The existing bucket, or null
  /// </returns>
  IBucketBase? GetBucket(string bucketName);
}

/// <summary>
/// The untyped common base class for all strongly typed
/// <see cref="IJsonBucket{T}"/>s in a <see cref="IJsonBucketStore"/>
/// as well as <see cref="IBlobBucket"/>s.
/// </summary>
public interface IBucketBase
{
  /// <summary>
  /// The name of the bucket
  /// </summary>
  string BucketName { get; }

  /// <summary>
  /// The type stored by the strongly typed implementation
  /// </summary>
  Type BucketType { get; }

  /// <summary>
  /// Remove all content from the bucket
  /// </summary>
  public void Erase();
}

/// <summary>
/// Extends <see cref="IBucketBase"/> with strongly typed key
/// related functionality
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IBucketBase<TKey>: IBucketBase
  where TKey : struct
{
  /// <summary>
  /// Enumerate the available keys. This is a function rather than
  /// a property to emphasize that this may be an expensive call.
  /// </summary>
  /// <returns></returns>
  IEnumerable<TKey> Keys();

  /// <summary>
  /// Checks if the element with the specified key is in the store
  /// (without actually loading it)
  /// </summary>
  bool ContainsKey(TKey key);
}


/// <summary>
/// Optional interface for IBucketStore: provides access to the filesystem
/// folder where the store lives
/// </summary>
public interface IHasFolder
{
  /// <summary>
  /// The file system folder where the item is materialized.
  /// </summary>
  string StorageFolder { get; }
}

/// <summary>
/// Extension methods and static functionality related to
/// BucketStores
/// </summary>
public static class BucketStoreExtensions
{

  /// <summary>
  /// Get the registered bucket and verify it contains items of type
  /// <typeparamref name="T"/>. Returns null if <paramref name="bucketName"/>
  /// is not known.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="bucketStore"></param>
  /// <param name="bucketName"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static IBucketBase? GetBucket<T>(
    this IBucketStore bucketStore,
    string bucketName)
    where T : class
  {
    if(!NamingRules.IsValidBucketName(bucketName))
    {
      throw new ArgumentException(
        $"Not a valid bucket name: '{bucketName}'");
    }
    var bucket = bucketStore.GetBucket(bucketName);
    if(bucket != null && bucket.BucketType != typeof(T))
    {
      throw new InvalidOperationException(
        $"Cannot retrieve bucket '{bucketName}' for type '{typeof(T).FullName}'" +
        $" because it is registered for type '{bucket.BucketType.FullName}'");
    }
    return bucket;
  }

  /// <summary>
  /// If the store supports <see cref="IBlobBucketStore"/> then this
  /// calls <see cref="IBlobBucketStore.GetBlobBucket(string, bool)"/>.
  /// Otherwise this throws a <see cref="NotSupportedException"/>.
  /// </summary>
  public static IBlobBucket? GetBlobBucket(
    this IBucketStore bucketStore,
    string bucketName,
    bool create)
  {
    if(bucketStore is not IBlobBucketStore blobBucketStore)
    {
      throw new NotSupportedException(
        "This bucket store does not support blob buckets");
    }
    return blobBucketStore.GetBlobBucket(bucketName, create);
  }

  /// <summary>
  /// If the store supports <see cref="IJsonBucketStore"/> then this
  /// calls <see cref="IJsonBucketStore.GetJsonBucket{T}(string, bool)"/>.
  /// Otherwise this throws a <see cref="NotSupportedException"/>.
  /// </summary>
  public static IJsonBucket<T>? GetJsonBucket<T>(
    this IBucketStore bucketStore,
    string bucketName, bool create = false)
    where T : class
  {
    if(bucketStore is not IJsonBucketStore jsonBucketStore)
    {
      throw new NotSupportedException(
        "This bucket store does not support json buckets");
    }
    return jsonBucketStore.GetJsonBucket<T>(bucketName, create);
  }

  /// <summary>
  /// If the store supports <see cref="ISingletonStore"/> then this
  /// calls <see cref="ISingletonStore.TryGetSingleton"/>.
  /// Otherwise this throws a <see cref="NotSupportedException"/>.
  /// </summary>
  public static bool TryGetSingleton<T>(
    this IBucketStore bucketStore,
    string? typename,
    [NotNullWhen(true)] out T? value,
    string key = "singleton")
    where T : class
  {
    if(bucketStore is not ISingletonStore singletonStore)
    {
      throw new NotSupportedException(
        "This bucket store does not support singleton operations");
    }
    return singletonStore.TryGetSingleton<T>(typename, out value, key);
  }

  /// <summary>
  /// If the store supports <see cref="ISingletonStore"/> then this
  /// calls <see cref="ISingletonStore.PutSingleton"/>.
  /// Otherwise this throws a <see cref="NotSupportedException"/>.
  /// </summary>
  public static void PutSingleton<T>(
    this IBucketStore bucketStore,
    string? typename,
    T? value,
    string key = "singleton")
    where T : class
  {
    if(bucketStore is not ISingletonStore singletonStore)
    {
      throw new NotSupportedException(
        "This bucket store does not support singleton operations");
    }
    singletonStore.PutSingleton<T>(typename, value, key);
  }

}


