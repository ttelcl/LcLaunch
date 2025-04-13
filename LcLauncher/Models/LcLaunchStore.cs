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

namespace LcLauncher.Models;

/// <summary>
/// Static extensions on <see cref="ILcLaunchStore"/>
/// </summary>
public static class LcLaunchStore
{
  /// <summary>
  /// Test if a rack name is valid, returning null on success
  /// or an error message on failure.
  /// </summary>
  public static string? TestValidRackName(
    string rackName)
  {
    if(string.IsNullOrWhiteSpace(rackName))
    {
      return "Rack name cannot be empty";
    }
    if(rackName.Contains(Path.DirectorySeparatorChar))
    {
      return "Rack name cannot contain path separators ('/' or '\\')";
    }
    if(rackName.Contains(Path.AltDirectorySeparatorChar))
    {
      return "Rack name cannot contain path separators ('/' or '\\')";
    }
    if(rackName.IndexOfAny([':', '\'', '"', '?', '*', '[', ']', '{', '}']) >= 0)
    {
      return 
        "Rack name cannot contain any of the following special characters ?*:'\"*";
    }
    if(rackName.Contains(".."))
    {
      return "Rack name cannot contain multiple consecutive '.' characters";
    }
    return null;
  }

  public static void ValidateRackName(
    string rackName)
  {
    var errorMessage = TestValidRackName(rackName);
    if(errorMessage == null)
    {
      return;
    }
    throw new InvalidOperationException(
      $"Invalid rack name: {errorMessage}");
  }

  public static bool RackExists(
    this ILcLaunchStore store,
    string rackName)
  {
    if(TestValidRackName(rackName) != null)
    {
      return false;
    }
    var rack = store.LoadRack(rackName);
    return rack != null;
  }

  public static RackData CreateRack(
    this ILcLaunchStore store,
    string rackName,
    bool overwrite)
  {
    ValidateRackName(rackName);
    if(!overwrite)
    {
      var existingRack = store.LoadRack(rackName);
      if(existingRack != null)
      {
        throw new InvalidOperationException(
          $"Rack '{rackName}' already exists");
      }
    }
    var emptyRack = new RackData([[], [], []]);
    Trace.TraceInformation(
      $"Creating new rack '{rackName}'");
    store.SaveRack(rackName, emptyRack);
    return emptyRack;
  }

  public static RackData LoadOrCreateRack(
    this ILcLaunchStore store,
    string rackName)
  {
    ValidateRackName(rackName);
    var existingRack = store.LoadRack(rackName);
    if(existingRack != null)
    {
      return existingRack;
    }
    return CreateRack(store, rackName, false);
  }
}
