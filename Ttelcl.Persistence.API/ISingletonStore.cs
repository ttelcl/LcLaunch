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
/// A store for storing a single object per "type".
/// A type here is a string, which hopefully corresponds
/// to a runtime type, but that is up to the client to
/// enforce.
/// </summary>
public interface ISingletonStore
{
  /// <summary>
  /// Find the value for <paramref name="typename"/>.
  /// </summary>
  /// <typeparam name="T">
  /// The runtime type to be found. Implementations should consider
  /// special-casing the case where this is byte[].
  /// </typeparam>
  /// <param name="typename">
  /// The logical name of the type. The design intent is that there is a 1-1
  /// mapping between <typeparamref name="T"/> and <paramref name="typename"/>,
  /// but clients are free to use it differently.
  /// If null, a type name is derived using
  /// <see cref="SingletonStore.DefaultTypeName{T}"/>
  /// This argument must satisfy <see cref="NamingRules.IsValidBucketName(string)"/>.
  /// </param>
  /// <param name="value">
  /// </param>
  /// <param name="key">
  /// An optional key for the item. Defaults to "singleton".
  /// </param>
  /// <returns></returns>
  bool TryGetSingleton<T>(
    string? typename,
    [NotNullWhen(true)] out T? value,
    string key = "singleton")
    where T: class;

  /// <summary>
  /// Store or delete the value for <paramref name="typename"/>.
  /// </summary>
  /// <typeparam name="T">
  /// The runtime type to be found. Implementations should consider
  /// special-casing the case where this is byte[].
  /// </typeparam>
  /// <param name="typename">
  /// The logical name of the type. The design intent is that there is a 1-1
  /// mapping between <typeparamref name="T"/> and <paramref name="typename"/>,
  /// but clients are free to use it differently.
  /// If null, a type name is derived using
  /// <see cref="SingletonStore.DefaultTypeName{T}"/>
  /// This argument must satisfy <see cref="NamingRules.IsValidBucketName(string)"/>.
  /// </param>
  /// <param name="value">
  /// The object to store, or null to delete.
  /// </param>
  /// <param name="key">
  /// An optional key for the item. Defaults to "singleton".
  /// </param>
  void PutSingleton<T>(
    string? typename,
    T? value,
    string key = "singleton")
    where T : class;

  /// <summary>
  /// Delete ALL singleton items in this store.
  /// </summary>
  void EraseSingletons();
}

/// <summary>
/// Extension methods and other static functionality related to
/// <see cref="ISingletonStore"/>
/// </summary>
public static class SingletonStore
{
  /// <summary>
  /// Return a "type name" for the type parameter <typeparamref name="T"/>.
  /// The returned value satisfies<see cref="NamingRules.IsValidBucketName(string)"/>. 
  /// </summary>
  public static string DefaultTypeName<T>()
    where T : class
  {
    var typeName =
      typeof(T).FullName
      ?? throw new InvalidOperationException(
        "This type has no name and therefore cannot be used in this store");
    return typeName.Replace('.', '-').ToLowerInvariant();
  }

}

