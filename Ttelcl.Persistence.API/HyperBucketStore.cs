/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ttelcl.Persistence.API;

/// <summary>
/// A store of <see cref="IBucketStore"/>s. This is where you plug
/// in implementation providers.
/// </summary>
public class HyperBucketStore
{
  private readonly List<IBucketProvider> _providers;
  private readonly Dictionary<string, IBucketProvider> _providerMap;
  private readonly Dictionary<StoreKey, IBucketStore> _storeCache;

  /// <summary>
  /// Create a new HyperBucketStore
  /// </summary>
  public HyperBucketStore(
    string rootFolder,
    IEnumerable<IBucketProvider> providers)
  {
    RootFolder = Path.GetFullPath(rootFolder);
    if(!Directory.Exists(RootFolder))
    {
      Directory.CreateDirectory(RootFolder);
    }
    _providers = providers.ToList();
    _providerMap =
      providers.ToDictionary(f => f.ProviderName);
    _storeCache = new Dictionary<StoreKey, IBucketStore>();
    if(!_providers.Any())
    {
      throw new ArgumentException(
        "Expecting at least one bucket provider implementation");
    }
    var badNames =
      _providers
      .Select(f => f.ProviderName)
      .Where(n => !NamingRules.IsValidProviderName(n))
      .ToList();
    if(badNames.Count != 0)
    {
      var list = String.Join(", ", badNames);
      throw new ArgumentException(
        $"Invalid provider names supplied. The following names are" +
        $" not valid provider names: {list}");
    }
  }

  /// <summary>
  /// The full path to the HyperBucketStore's root folder. This is a filesystem
  /// folder, even when using non-filesystem providers.
  /// </summary>
  public string RootFolder { get; }

  /// <summary>
  /// The default bucket store factory (the first one in the list)
  /// </summary>
  public IBucketProvider DefaultProvider => _providers[0];

  /// <summary>
  /// The default provider name (
  /// <see cref="DefaultProvider"/>.<see cref="IBucketProvider.ProviderName"/>).
  /// </summary>
  public string DefaultProviderName => DefaultProvider.ProviderName;

  /// <summary>
  /// The list of all available bucket store factories (there will be at least one)
  /// </summary>
  public IReadOnlyCollection<string> ProviderNames => _providerMap.Keys;

  /// <summary>
  /// The map of provider names to providers.
  /// </summary>
  public IReadOnlyDictionary<string, IBucketProvider> Providers => _providerMap;

  /// <summary>
  /// Get an <see cref="IBucketStore"/> instance. If the same store was
  /// requested before a cached instance is returned. Otherwise the
  /// provider is invoked to create a new instance.
  /// </summary>
  /// <param name="storeName">
  /// The user friendly name of the store.
  /// </param>
  /// <param name="storeTag">
  /// The optional 'tag' for the store to indicate the domain of the
  /// data that will be stored in the store's buckets.
  /// </param>
  /// <param name="providerName">
  /// The name of the provider that determines the store's implementation.
  /// Defaults to <see cref="DefaultProviderName"/>.
  /// </param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public IBucketStore GetStore(
    string storeName,
    string storeTag = "store",
    string? providerName = null)
  {
    providerName ??= DefaultProviderName;
    if(!_providerMap.TryGetValue(providerName, out var provider))
    {
      throw new InvalidOperationException(
        $"Unknown / unregistered bucket store provider '{providerName}'");
    }
    // the following gets the unique key for the store and validates the arguments
    var storeKey = GetStoreKey(storeName, providerName, storeTag);
    if(!_storeCache.TryGetValue(storeKey, out var store))
    {
      store = provider.GetBucketStore(RootFolder, storeName, storeTag);
      _storeCache.Add(storeKey, store);
    }
    return store;
  }

  /// <summary>
  /// Get the store as defined by <paramref name="storeKey"/>. Calls
  /// <see cref="GetStore(string, string, string?)"/> with the values found
  /// in the key.
  /// </summary>
  /// <param name="storeKey"></param>
  /// <returns></returns>
  public IBucketStore GetStore(StoreKey storeKey)
  {
    return GetStore(
      storeKey.StoreName,
      storeKey.StoreTag,
      storeKey.ProviderName);
  }

  /// <summary>
  /// Return a string uniquely identifying a store with the given
  /// <paramref name="storeName"/>, bound to the given
  /// <paramref name="providerName"/> and using the given
  /// <paramref name="storeTag"/>.
  /// </summary>
  /// <param name="storeName">
  /// The simple name of the store, as suitable for use in
  /// user interfaces.
  /// </param>
  /// <param name="storeTag">
  /// A tag that identifies the purpose of the store, similar
  /// to a database schema.
  /// </param>
  /// <param name="providerName">
  /// A very short name that identifies the implementation used.
  /// </param>
  /// <returns></returns>
  /// <exception cref="ArgumentException"></exception>
  public static StoreKey GetStoreKey(
    string storeName,
    string providerName,
    string storeTag = "store")
  {
    if(!NamingRules.IsValidStoreName(storeName))
    {
      throw new ArgumentException(
        $"Not a valid bucket store name: '{storeName}'");
    }
    if(!NamingRules.IsValidStoreTagName(storeTag))
    {
      throw new ArgumentException(
        $"Not a valid bucket store tag name: '{storeTag}'");
    }
    if(!NamingRules.IsValidProviderName(providerName))
    {
      throw new ArgumentException(
        $"Not a valid bucket store provider name: '{providerName}'");
    }
    return new StoreKey(providerName, storeTag, storeName);
  }

  /// <summary>
  /// Enumerate child folders in <see cref="RootFolder"/> whose name matches
  /// the expectation for a bucket store folder. Only store keys for
  /// providers known in this <see cref="HyperBucketStore"/> are returned.
  /// </summary>
  /// <returns></returns>
  public IEnumerable<StoreKey> FindFolderStores()
  {
    foreach(var childFolder in Directory.EnumerateDirectories(
      RootFolder, "*--*--*"))
    {
      var shortName = Path.GetFileName(childFolder);
      var storeKey = StoreKey.TryParse(shortName);
      if(storeKey != null && _providerMap.ContainsKey(storeKey.ProviderName))
      {
        yield return storeKey;
      }
    }
  }

  /// <summary>
  /// Like <see cref="FindFolderStores()"/>, but only return keys with
  /// <paramref name="storeTag"/> as their <see cref="StoreKey.StoreTag"/>
  /// value.
  /// </summary>
  public IEnumerable<StoreKey> FindFolderStores(string storeTag)
  {
    return FindFolderStores().Where(sk => sk.StoreTag == storeTag);
  }

  /// <summary>
  /// Check if the argument is a known storage provider name
  /// </summary>
  public bool IsKnownProviderName(string providerName)
  {
    return _providerMap.ContainsKey(providerName);
  }

}
