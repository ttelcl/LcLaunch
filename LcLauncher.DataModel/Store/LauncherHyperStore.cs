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
    if(!NamingRules.IsValidStoreName(rackName))
    {
      throw new ArgumentException(
        $"'{rackName}' is not a valid name for a rack.");
    }
    providerName ??= Backing.DefaultProviderName;
    if(!Backing.IsKnownProviderName(providerName))
    {
      throw new ArgumentException(
        $"'{providerName}' is not a recognized storage provider name");
    }
    return new StoreKey(
      providerName,
      "rack",
      rackName);
  }

  /// <summary>
  /// Returns all rack keys for racks with the given name, ordered by provider
  /// index. While the expectation is that zero or one key is returned, it is
  /// possible that multiple matches are found (with different providers).
  /// As such, this should be considered an advanced method.
  /// Consider using <see cref="FindRackByName(string)"/> instead
  /// Name matching is case insensitive.
  /// </summary>
  /// <param name="rackName">
  /// </param>
  /// <returns></returns>
  public List<StoreKey> FindRacksByName(string rackName)
  {
    var keys =
      FindRackStores()
      .Where(k => k.StoreName.Equals(
        rackName, StringComparison.InvariantCultureIgnoreCase))
      .ToList();
    var sorted = new List<StoreKey>();
    foreach(var providerName in Backing.ProviderNames)
    {
      var match = keys.FirstOrDefault(k => k.ProviderName == providerName);
      if(match != null)
      {
        sorted.Add(match);
      }
    }
    return sorted;
  }

  /// <summary>
  /// Finds the rack key for the known rack with the (case insensitive)
  /// name <paramref name="rackName"/>. If there are multiple matches the
  /// one with the provider that appears earliest in
  /// <see cref="HyperBucketStore.ProviderNames"/> is returned.
  /// </summary>
  /// <param name="rackName"></param>
  /// <returns></returns>
  public StoreKey? FindRackByName(string rackName)
  {
    return FindRacksByName(rackName).FirstOrDefault();
  }

  /// <summary>
  /// The default store folder TEST VERSION
  /// </summary>
  public static string DefaultStoreFolder { get; } =
    Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "Lcl",
      "launcher");

  /// <summary>
  /// Check if any rack with name '<paramref name="rackName"/>' exists, and
  /// if not create one, using the given <paramref name="providerName"/>
  /// (or the default provider if null)
  /// </summary>
  /// <param name="rackName">
  /// The rack name. Must satisfy <see cref="NamingRules.IsValidStoreName(string)"/>
  /// </param>
  /// <param name="providerName">
  /// The name of the provider (one of the names in
  /// <see cref="HyperBucketStore.ProviderNames"/> of <see cref="Backing"/>). 
  /// Or null to use the default.
  /// </param>
  /// <returns>
  /// True if a rack was created, false if it was found to exist already.
  /// </returns>
  public bool CreateRackIfNotExists(
    string rackName,
    string? providerName = null)
  {
    // Do this first, even if the result is not needed, since it validates
    // arguments, so the behaviour for invalid arguments is consistent.
    var key = GetRackKey(rackName, providerName);
    var existingKey = FindRackByName(rackName);
    if(existingKey != null)
    {
      // a matching rack already exists
      return false;
    }
    // This creates the actual rack folder:
    var rackStore = GetRackStore(key);
    // This creates an empty rack with 3 columns, one with an empty shelf
    // and two empty
    var rack = rackStore.GetRack();
    return true;
  }
}
