/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LcLauncher.IconUpdates;

/// <summary>
/// An object that can have an icon.
/// The most important part of this is a unique ID
/// to help detect duplicate <see cref="IconLoadJob"/>s for the same
/// target.
/// </summary>
public interface IIconHost
{
  /// <summary>
  /// The ID uniquely identifying this icon host.
  /// Attempting to queue a load job for the same ID
  /// is assumed to be a duplicate and will be ignored.
  /// </summary>
  Guid IconHostId { get; }

}

