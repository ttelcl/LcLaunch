/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttelcl.Persistence.API;

/// <summary>
/// The factory / provider instance that can create <see cref="IBucketStore"/>
/// implementations. Implementations of this are the connection between
/// implementation-neutral APIs in this namespace (Ttelcl.Persistence.API)
/// and implementations. One or more implementations of this interface
/// are provided to the constructor of <see cref="HyperBucketStore"/>,
/// or such an instance can be used directly.
/// </summary>
public interface IBucketProvider
{
  /// <summary>
  /// The name identifying this factory. This is expected to be
  /// an extremely short name, canonically consisting of two 
  /// lower case letters. This string must satisfy
  /// <see cref="NamingRules.IsValidProviderName(string)"/>
  /// </summary>
  string ProviderName { get; }

  /// <summary>
  /// Create a new <see cref="IBucketStore"/> instance.
  /// Consider using
  /// <see cref="HyperBucketStore.GetStoreKey(string, string, string)"/>
  /// to construct names and name parts for resources used by the
  /// bucket store.
  /// </summary>
  /// <param name="hostFolder">
  /// If the implementation needs a file system folder, it can create
  /// one below this folder.
  /// </param>
  /// <param name="storeName">
  /// The short name of the store. Must satisfy
  /// <see cref="NamingRules.IsValidStoreName(string)"/>.
  /// </param>
  /// <param name="tag">
  /// The type tag for use in the naming of the bucket store's
  /// resources. By default "store". Must satisfy
  /// <see cref="NamingRules.IsValidStoreTagName(string)"/>.
  /// </param>
  /// <returns></returns>
  IBucketStore GetBucketStore(
    string hostFolder,
    string storeName,
    string tag = "store");
}
