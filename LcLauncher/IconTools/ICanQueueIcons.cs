/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.IconTools;

/// <summary>
/// An object that directly or indirectly has icons whose loading
/// can be queued
/// </summary>
public interface ICanQueueIcons
{
  
  /// <summary>
  /// Load the embedded icons
  /// </summary>
  /// <param name="regenerate">
  /// Don't just load icons from cache, regenerate them.
  /// </param>
  void QueueIcons(bool regenerate);
}

