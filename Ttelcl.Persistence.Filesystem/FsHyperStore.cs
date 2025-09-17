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
/// Description of FsHyperStore
/// </summary>
public class FsHyperStore: IHyperBucketStore
{
  /// <summary>
  /// Create a new FsHyperStore
  /// </summary>
  public FsHyperStore(
    string rootFolder)
  {
    RootFolder = Path.GetFullPath(rootFolder);
  }

  /// <summary>
  /// The root folder. Each child bucket store will be created in
  /// its own child folder.
  /// </summary>
  public string RootFolder { get; }

  /// <inheritdoc/>
  public IBucketStore GetStore(string storeName)
  {
    if(!BucketStoreExtensions.IsValidBucketName(storeName))
    {
      throw new ArgumentException(
        $"Not a valid child store name: '{storeName}'");
    }
    return new FsBucketStore(Path.Combine(RootFolder, storeName));
  }

}
