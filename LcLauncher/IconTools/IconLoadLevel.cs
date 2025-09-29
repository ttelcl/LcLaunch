/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcLauncher.IconTools;

/// <summary>
/// The different tiers of icon loading as used by
/// <see cref="IconLoader"/>.
/// </summary>
public enum IconLoadLevel
{
  /// <summary>
  /// Load the icon from memory cache. Do not check the disk
  /// cache if missing
  /// This requires the Icon ID to be available.
  /// </summary>
  MemoryCache = 0,

  /// <summary>
  /// If missing from the memory cache, fill the memory cache
  /// from disk cache. Do not attempt to generate the icon 
  /// from the icon source.
  /// This requires the Icon ID to be available.
  /// </summary>
  DiskCache = 1,

  /// <summary>
  /// If missing from the disk cache, have the operating system
  /// generate the icon from the icon source.
  /// This requires the icon source to be available (and can create
  /// the icon ID from that)
  /// </summary>
  System = 2,
}
