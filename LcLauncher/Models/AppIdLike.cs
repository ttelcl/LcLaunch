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

namespace LcLauncher.Models;

// Largely based on https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/package-identity-overview

/// <summary>
/// Describes an app-id like string and breaks it into parts
/// </summary>
public class AppIdLike
{
  /// <summary>
  /// Create a new AppIdLike
  /// </summary>
  private AppIdLike(
    string familyName,
    string publisherId,
    string applicationName)
  {
    FamilyName = familyName;
    PublisherId = publisherId;
    ApplicationName = applicationName;
    FullName = FamilyName + "_" + PublisherId + "!" + ApplicationName;
  }

  /// <summary>
  /// Try breaking up a full app name into its parts. Returns the 
  /// parts upon success, or null on failure
  /// </summary>
  public static AppIdLike? TryParse(
    string fullName)
  {
    var match = Regex.Match(
      fullName,
      "^([-.a-zA-Z0-9]{3,50})_([a-hj-km-np-tv-z0-9]{13})!([-.a-zA-Z0-9]{3,50})$");
    if(match.Success)
    { 
      return new AppIdLike(
        match.Groups[1].Value,
        match.Groups[2].Value,
        match.Groups[3].Value);
    }
    return null;
  }

  public string FamilyName { get; }

  public string PublisherId { get; }

  public string ApplicationName { get; }

  public string FullName { get; }

}
