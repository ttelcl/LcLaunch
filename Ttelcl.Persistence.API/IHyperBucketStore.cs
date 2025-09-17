/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ttelcl.Persistence.API;

/// <summary>
/// A store containing multiple bucketstore children,
/// identified by name. The children can be any type of bucket store
/// </summary>
public interface IHyperBucketStore
{
  /// <summary>
  /// Get a generic store by store name. Names must be valid
  /// according to <see cref="BucketStoreExtensions.IsValidBucketName(string)"/>
  /// </summary>
  /// <param name="storeName"></param>
  /// <returns></returns>
  IBucketStore GetStore(string storeName);
}

