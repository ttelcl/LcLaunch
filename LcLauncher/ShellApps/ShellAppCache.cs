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

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;

using LcLauncher.Main;

namespace LcLauncher.ShellApps;

/// <summary>
/// Description of ShellAppCache
/// </summary>
public class ShellAppCache
{
  private readonly Dictionary<string, ShellAppDescriptor> _descriptorCache;
  private long _lastReloadTicks = 0L;

  /// <summary>
  /// Create a new ShellAppCache
  /// </summary>
  /// <param name="prefill">
  /// If true, immediately call <see cref="Refill"/>. This may cause slowness.
  /// </param>
  public ShellAppCache(
    bool prefill)
  {
    _descriptorCache = new Dictionary<string, ShellAppDescriptor>();
    if(prefill)
    {
      Refill(TimeSpan.Zero);
    }
  }

  /// <summary>
  /// Return the collection of all descriptors in this cache
  /// (in no particular order)
  /// </summary>
  public IEnumerable<ShellAppDescriptor> Descriptors {
    get {
      // Avoid duplicates by only emitting the cases where
      // the key is the ParsingName, not the FullParsingName
      foreach(var kvp in _descriptorCache)
      {
        if(kvp.Key == kvp.Value.ParsingName)
        {
          yield return kvp.Value;
        }
      }
    }
  }

  /// <summary>
  /// Completely (re)fill the cache (and clear entries that are no longer valid)
  /// </summary>
  public void Refill(
    TimeSpan minAge)
  {
    var currentTicks = DateTime.UtcNow.Ticks;
    var minAgeTicks = minAge.Ticks;
    var ageTicks = currentTicks - _lastReloadTicks;
    if(ageTicks < minAgeTicks)
    {
      Trace.TraceInformation("Skipping App reload because it is recent");
      return;
    }
    _lastReloadTicks = currentTicks;
    var t0 = DateTime.UtcNow;
    using var appsFolder =
      (ShellObject)KnownFolderHelper.FromKnownFolderId(AppsFolderId);
    var appsVf = (IKnownFolder)appsFolder;
    var tmpList = new List<ShellAppDescriptor>();
    foreach(var app in appsVf)
    {
      var descriptor = ShellAppDescriptor.FromShellObject(app);
      tmpList.Add(descriptor);
    }
    _descriptorCache.Clear();
    foreach(var descriptor in tmpList)
    {
      Put(descriptor);
    }
    var t1 = DateTime.UtcNow;
    var duration = t1 - t0;
    Trace.TraceInformation(
      $"Refill took {duration.TotalSeconds:F7} seconds");
  }

  /// <summary>
  /// Find the app descriptor for the given full or partial parsingname
  /// in this cache.
  /// </summary>
  /// <param name="parsingName">
  /// The full or partial parsing name. For true apps this would be the
  /// application id.
  /// </param>
  /// <returns>
  /// The app descriptor if already cached, or null if not
  /// </returns>
  public ShellAppDescriptor? Find(string parsingName)
  {
    return _descriptorCache.TryGetValue(parsingName, out var descriptor)
      ? descriptor
      : null;
  }

  /// <summary>
  /// If the descriptor with the given full or partial parsingname is in the
  /// cache, then return it.
  /// Otherwise try to create it; if successful put it in the cache and return
  /// it, otherwise return null.
  /// </summary>
  /// <param name="parsingName"></param>
  /// <returns></returns>
  public ShellAppDescriptor? FindOrTryCreate(string parsingName)
  {
    var descriptor = Find(parsingName);
    if(descriptor != null)
    {
      return descriptor;
    }
    descriptor = ShellAppDescriptor.TryFromParsingName(parsingName);
    if(descriptor != null)
    {
      Put(descriptor);
    }
    return descriptor;
  }

  public void Put(ShellAppDescriptor descriptor)
  {
    _descriptorCache[descriptor.ParsingName] = descriptor;
    _descriptorCache[descriptor.FullParsingName] = descriptor;
  }

  private readonly Guid AppsFolderId =
    new("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");

}
