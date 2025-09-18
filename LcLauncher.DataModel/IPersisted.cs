/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.DataModel;

/// <summary>
/// An item that tracks a dirty state and can save itself
/// to some kind of persistent storage.
/// </summary>
public interface IPersisted
{

  /// <summary>
  /// True if this item has been marked as dirty and should be
  /// saved at some point. Set by <see cref="MarkDirty"/> and
  /// cleared by <see cref="SaveIfDirty"/>.
  /// </summary>
  bool IsDirty { get; }

  /// <summary>
  /// Mark this item as dirty, setting the <see cref="IsDirty"/>
  /// flag.
  /// </summary>
  void MarkDirty();

  /// <summary>
  /// Save this item if it is dirty and clear the <see cref="IsDirty"/>
  /// flag.
  /// </summary>
  void SaveIfDirty();
}

