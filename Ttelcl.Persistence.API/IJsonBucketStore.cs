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
  /// Enumerate the available keys. This is a function rather than
  /// a property to emphasize that this may be an expensive call.
  /// </summary>
  /// <returns></returns>
  IEnumerable<TickId> Keys();
}

/// <summary>
/// Stores and retrieves JSON serializable instances. 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IJsonBucket<T>: IBucketBase
  where T : class
{
  /// <summary>
  /// Store an item in the bucket, or retrieve it, or delete it.
  /// If not found, null is returned.
  /// By storing null the entry is deleted (if it existed).
  /// </summary>
  /// <param name="key"></param>
  /// <returns></returns>
  T? this[TickId key] { get; set; }
}

/// <summary>
/// An abstract data storage API
/// </summary>
public interface IJsonBucketStore
{
  /// <summary>
  /// Get the bucket within this <see cref="IJsonBucketStore"/> with
  /// the name <paramref name="bucketName"/> that stores items of type
  /// <typeparamref name="T"/>. If the named bucket is found, but its
  /// type does not match <typeparamref name="T"/>, an exception is
  /// thrown. If the named bucket is not found the behaviour depends
  /// on the <paramref name="create"/> parameter. If true, a new
  /// bucket is created and returned. If false, null is returned.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="bucketName"></param>
  /// <param name="create"></param>
  /// <returns></returns>
  IJsonBucket<T> GetBucket<T>(string bucketName, bool create)
    where T : class;
}

/// <summary>
/// Extension methods on <see cref="IJsonBucket{T}"/>
/// </summary>
public static class JsonBucketExtensions
{
  
  /// <summary>
  /// Store <paramref name="data"/> into the <paramref name="bucket"/>
  /// at key <paramref name="id"/>.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="bucket"></param>
  /// <param name="id"></param>
  /// <param name="data"></param>
  public static void Put<T>(
    this IJsonBucket<T> bucket,
    TickId id,
    T data)
    where T : class
  {
    bucket[id] = data;
  }

  /// <summary>
  /// Delete the entry at key <paramref name="id"/> in
  /// <paramref name="bucket"/> (if it existed)
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="bucket"></param>
  /// <param name="id"></param>
  public static void Delete<T>(
    this IJsonBucket<T> bucket,
    TickId id)
    where T : class
  {
    bucket[id] = null;
  }

  /// <summary>
  /// Return the entry at key <paramref name="id"/> in this
  /// <paramref name="bucket"/>. Returns null if not found
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="bucket"></param>
  /// <param name="id"></param>
  /// <returns></returns>
  public static T? Find<T>(
    this IJsonBucket<T> bucket,
    TickId id)
    where T : class
  {
    return bucket[id];
  }

  /// <summary>
  /// Find the entry at key <paramref name="id"/> in this
  /// <paramref name="bucket"/>. If successful, the retrieved
  /// value is returned in <paramref name="item"/> and true
  /// is returned. If not succesful, <paramref name="item"/>
  /// is set to null and false is returned.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="bucket"></param>
  /// <param name="id"></param>
  /// <param name="item"></param>
  /// <returns></returns>
  public static bool TryFind<T>(
    this IJsonBucket<T> bucket,
    TickId id,
    [NotNullWhen(true)] out T? item)
    where T : class
  {
    item = bucket[id];
    return item != null;
  }


}

