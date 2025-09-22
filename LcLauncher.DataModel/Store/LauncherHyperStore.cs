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

namespace LcLauncher.DataModel.Store;

/// <summary>
/// A wrapper around <see cref="HyperBucketStore"/> with an API
/// specialized to rack data.
/// </summary>
public class LauncherHyperStore
{
  private readonly Dictionary<StoreKey, LauncherRackStore> _rackStoreCache;

  /// <summary>
  /// Create a new LauncherHyperStore
  /// </summary>
  /// <param name="backing">
  /// The hyper bucket store, including configured
  /// bucket store providers.
  /// </param>
  public LauncherHyperStore(
    HyperBucketStore backing)
  {
    Backing = backing;
    _rackStoreCache = [];
  }

  /// <summary>
  /// The hyperstore, corresponding to the root folder within
  /// which the individual rack store folders will live.
  /// </summary>
  public HyperBucketStore Backing { get; }

  /// <summary>
  /// Returns a list of store keys for rack data stores
  /// (assuming that store implementations use the expected
  /// folder names for child stores)
  /// </summary>
  /// <returns></returns>
  public IEnumerable<StoreKey> FindRackStores()
  {
    return Backing.FindFolderStores("rack");
  }

  /// <summary>
  /// Get the <see cref="LauncherRackStore"/> for the given key.
  /// If not found, this creates a new rack.
  /// </summary>
  /// <param name="key"></param>
  /// <returns></returns>
  public LauncherRackStore GetRackStore(StoreKey key)
  {
    if(_rackStoreCache.TryGetValue(key, out var rackStore))
    {
      return rackStore;
    }
    rackStore = new LauncherRackStore(this, key);
    _rackStoreCache.Add(key, rackStore);
    return rackStore;
  }

  /// <summary>
  /// Get the rack key given the specified <paramref name="rackName"/>.
  /// </summary>
  /// <param name="rackName">
  /// The name of the rack
  /// </param>
  /// <param name="providerName">
  /// The provider name, known in <see cref="Backing"/>. If null or omitted,
  /// the default persistence provider in <see cref="Backing"/> is used.
  /// </param>
  /// <returns></returns>
  public StoreKey GetRackKey(
    string rackName,
    string? providerName = null)
  {
    return new StoreKey(
      providerName ?? Backing.DefaultProviderName,
      "rack",
      rackName);
  }

  /// <summary>
  /// The default store folder TEST VERSION
  /// </summary>
  public static string DefaultStoreFolder { get; } =
    Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "Lcl",
      "launcher");

}
