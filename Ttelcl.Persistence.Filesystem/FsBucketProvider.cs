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
/// Implements <see cref="IBucketProvider"/> in the filesystem
/// </summary>
public class FsBucketProvider: IBucketProvider
{
  /// <summary>
  /// Create a new <see cref="FsBucketProvider"/>
  /// </summary>
  public FsBucketProvider()
  {
  }

  /// <summary>
  /// get this factory's name: "fs"
  /// </summary>
  public string ProviderName => "fs";

  /// <summary>
  /// Create a new <see cref="FsBucketStore"/> instance. Implements
  /// <see cref="IBucketProvider.GetBucketStore(string, string, string)"/>.
  /// </summary>
  /// <param name="hostFolder">
  /// The parent folder in which the store folder will live.
  /// </param>
  /// <param name="storeName">
  /// The user friendly name of the store
  /// </param>
  /// <param name="tag">
  /// The tag indicating the kind of data that will be stored in the store.
  /// </param>
  /// <returns></returns>
  public IBucketStore GetBucketStore(
    string hostFolder, string storeName, string tag = "store")
  {
    var storeKey = HyperBucketStore.GetStoreKey(
      storeName, ProviderName, tag);
    var storeFolderName =
      Path.Combine(hostFolder, storeKey.ToString());
    return new FsBucketStore(storeFolderName);
  }
}
