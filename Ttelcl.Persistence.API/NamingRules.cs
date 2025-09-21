/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ttelcl.Persistence.API;

/// <summary>
/// Provides naming rules for the different concepts in this
/// library
/// </summary>
public static partial class NamingRules
{
  /// <summary>
  /// Test if a name is a valid bucket name. A bucket name
  /// is a series of one or more identifiers separated by
  /// '-' or '_'. 
  /// </summary>
  public static bool IsValidBucketName(string name)
  {
    return BucketNameRegex().IsMatch(name);
  }

  /// <summary>
  /// Test if a name is a valid bucket store name
  /// (the rule is the same as <see cref="IsValidBucketName(string)"/>)
  /// </summary>
  public static bool IsValidStoreName(string name)
  {
    return BucketNameRegex().IsMatch(name);
  }

  /// <summary>
  /// Test if a name is a valid bucket store tag name.
  /// A store tag name is a single identifier (no '-' or '_')
  /// </summary>
  public static bool IsValidStoreTagName(string name)
  {
    return StoreTagRegex().IsMatch(name);
  }

  /// <summary>
  /// Test if a name is a valid provider (factory) name.
  /// A provider name is a single identifier no longer than 4 characters,
  /// (typically two characters long).
  /// </summary>
  /// <param name="name"></param>
  /// <returns></returns>
  public static bool IsValidProviderName(string name)
  {
    return ProviderNameRegex().IsMatch(name);
  }

  /// <summary>
  /// test if a name is a valid singleton key name.
  /// A singleton key name takes the form of a bucket name
  /// (<see cref="IsValidBucketName(string)"/>), optionally
  /// prefixed by a store tag (<see cref="IsValidStoreTagName(string)"/>)
  /// separated by '--'.
  /// </summary>
  /// <param name="name"></param>
  /// <returns></returns>
  public static bool IsValidSingletonKey(string name)
  {
    return SingletonKeyRegex().IsMatch(name);
  }

  [GeneratedRegex(
    @"^[a-z][a-z0-9]{0,3}$",
    RegexOptions.IgnoreCase,
    "")] // Culture name empty -> Invariant Culture
  private static partial Regex ProviderNameRegex();

  [GeneratedRegex(
    @"^[a-z][a-z0-9]*$",
    RegexOptions.IgnoreCase,
    "")] // Culture name empty -> Invariant Culture
  private static partial Regex StoreTagRegex();

  [GeneratedRegex(
    @"^[a-z][a-z0-9]*([-_][a-z0-9]+)*$",
    RegexOptions.IgnoreCase,
    "")] // Culture name empty -> Invariant Culture
  private static partial Regex BucketNameRegex();

  [GeneratedRegex(
    @"^([a-z][a-z0-9]*--)?[a-z][a-z0-9]*([-_][a-z0-9]+)*$",
    RegexOptions.IgnoreCase,
    "")] // Culture name empty -> Invariant Culture
  private static partial Regex SingletonKeyRegex();
}
