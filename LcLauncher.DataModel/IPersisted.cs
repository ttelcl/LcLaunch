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
public interface IPersisted: IDirtyBase
{

  /// <summary>
  /// Save this item if it is dirty, and clear the
  /// <see cref="IDirtyBase.IsDirty"/> flag.
  /// </summary>
  void SaveIfDirty();
}

/// <summary>
/// An item that tracks a dirty state, can mark itself as dirty.
/// Subinterfaces specify how to clear the dirty state
/// </summary>
public interface IDirtyBase
{
  /// <summary>
  /// True if this item has been marked as dirty and should be
  /// saved at some point. Set by <see cref="MarkDirty"/> and
  /// cleared by <see cref="IPersisted.SaveIfDirty"/>.
  /// </summary>
  bool IsDirty { get; }

  /// <summary>
  /// Mark this item as dirty, setting the <see cref="IsDirty"/>
  /// flag.
  /// </summary>
  void MarkDirty();
}

/// <summary>
/// Extends <see cref="IDirtyBase"/> with an explicit 
/// <see cref="MarkClean"/> method. Implementing this interface
/// indicates the client that modifies the object is responsible
/// for setting and clearing the dirty flag.
/// </summary>
public interface IDirty: IDirtyBase
{
  /// <summary>
  /// Mark this item as clean, clearing the <see cref="IDirtyBase.IsDirty"/>
  /// flag.
  /// </summary>
  void MarkClean();
}

/// <summary>
/// An object that is implicitly aware of zero or more <see cref="IDirty"/>
/// and/or <see cref="IDeepDirty"/> implementing child objects that knows
/// how to check if any child is dirty
/// </summary>
public interface IDeepDirty: IDirty
{
  /// <summary>
  /// Check if this object itself is dirty, or any child object is
  /// dirty (recursively).
  /// </summary>
  /// <returns></returns>
  bool IsDeepDirty();
}

