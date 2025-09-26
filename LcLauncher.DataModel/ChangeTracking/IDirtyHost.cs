/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.DataModel.ChangeTracking;

/// <summary>
/// An object that knows how to mark itself or a suitable parent
/// as "dirty" (in need of saving). Usually exposed through 
/// <see cref="IDirtyHost"/> or <see cref="IDirtyPart"/>
/// </summary>
public interface IDirtyMarkable
{
  /// <summary>
  /// Mark the appropriate <see cref="IDirtyHost"/> as dirty.
  /// </summary>
  void MarkAsDirty();
}

/// <summary>
/// An object that exposes a flag indicating it is "dirty" and
/// needs saving. In LcLaunch this is implemented by the ViewModels
/// that handle the models that in turn wrap persistable entities.
/// </summary>
public interface IDirtyHost: IDirtyMarkable
{
  /// <summary>
  /// True if this object is in need of saving. Setting this flag to
  /// true is accomplished through <see cref="IDirtyMarkable.MarkAsDirty"/>
  /// on this object itself or a child part
  /// </summary>
  bool IsDirty { get; }

  /// <summary>
  /// Save this object and clear the <see cref="IsDirty"/> flag.
  /// </summary>
  /// <param name="ifDirty">
  /// Only save if the <see cref="IsDirty"/> flag is true.
  /// </param>
  void Save(bool ifDirty = true);
}

/// <summary>
/// A part that can be marked as dirty, but doing so actually marks
/// an iten higher in the hierarchy as dirty
/// </summary>
public interface IDirtyPart: IDirtyMarkable
{
  /// <summary>
  /// The dirtyness tracking host that actually will be marked as dirty
  /// when this part is marked as dirty.
  /// In some cases the <see cref="IDirtyPart"/> may be disconnected and
  /// this would be null.
  /// </summary>
  IDirtyHost? DirtyHost { get; }
}

