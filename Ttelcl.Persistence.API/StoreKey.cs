/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ttelcl.Persistence.API;

/// <summary>
/// Identifies a <see cref="IBucketStore"/> within a
/// <see cref="HyperBucketStore"/>.
/// </summary>
public record StoreKey(
  string ProviderName,
  string StoreTag,
  string StoreName)
{
  /// <summary>
  /// Parse a string back to a <see cref="StoreKey"/> from
  /// the form created by <see cref="ToString"/>. Returns null
  /// if it is not valid.
  /// </summary>
  public static StoreKey? TryParse(string fullName)
  {
    var parts = fullName.Split("--");
    if(parts.Length != 3)
    {
      return null;
    }
    if(NamingRules.IsValidProviderName(parts[0])
      && NamingRules.IsValidStoreTagName(parts[1])
      && NamingRules.IsValidStoreName(parts[2]))
    {
      return new StoreKey(parts[0], parts[1], parts[2]);
    }
    return null;
  }
  
  /// <summary>
  /// Returns a string representation of the store key.
  /// In practice, this is used as a folder name.
  /// </summary>
  /// <returns></returns>
  public override string ToString()
  {
    return $"{ProviderName}--{StoreTag}--{StoreName}";
  }
}
