/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ttelcl.Persistence.API;

namespace LcLauncher.DataModel.Entities.Store;

/// <summary>
/// Description of LauncherHyperStore
/// </summary>
public class LauncherHyperStore
{
  /// <summary>
  /// Create a new LauncherHyperStore
  /// </summary>
  public LauncherHyperStore(
    IHyperBucketStore bucketStore)
  {
    BucketStore = bucketStore;
  }

  public IHyperBucketStore BucketStore { get; }

  /*
   * Stuff to do here! Find a way to enumerate. Maybe use a dedicated
   * tag instead of "store--"
   */

}
