/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LcLauncher.WpfUtilities;

public interface IWrappedProperty<T>
{
  /// <summary>
  /// Get the value
  /// </summary>
  /// <returns></returns>
  T GetValue();

  /// <summary>
  /// If the value is different from the existing value then set it and
  /// return true. Else return false
  /// </summary>
  /// <param name="value">
  /// The new value to set
  /// </param>
  /// <returns>
  /// True if changed, false if <paramref name="value"/> was the same
  /// as what <see cref="GetValue"/> returned.
  /// </returns>
  bool ChangeValue(T value);
}

/// <summary>
/// Wraps a property (or something behaving as a property). This version
/// takes delegates that implement the getter and changer, and the type
/// argument is unconstrained.
/// </summary>
public class WrappedProperty<T>: IWrappedProperty<T>
{
  private readonly Func<T> _getter;
  private readonly Func<T, bool> _changer;

  /// <summary>
  /// Create a new WrappedProperty
  /// </summary>
  /// <param name="getter">
  /// The function that returns the target value
  /// </param>
  /// <param name="changer">
  /// A function that checks if the provided value is different from
  /// what it was, and changes it if it is different. Returns true if changed.
  /// </param>
  public WrappedProperty(
    Func<T> getter,
    Func<T, bool> changer)
  {
    _getter = getter;
    _changer = changer;
  }

  /// <inheritdoc/>
  public T GetValue()
  {
    return _getter();
  }

  /// <inheritdoc/>
  public bool ChangeValue(T value)
  {
    return _changer(value);
  }
}

/// <summary>
/// This implements <see cref="IWrappedProperty{T}"/> for something behaving
/// as a value type, using a getter and setter delegate. <see cref="ChangeValue(T)"/> 
/// compares old and new values using EqualityComparer{T}.Default.Equals().
/// </summary>
/// <typeparam name="T"></typeparam>
public class WrappedValueProperty<T>: IWrappedProperty<T>
{
  private readonly Func<T> _getter;
  private readonly Action<T> _setter;

  public WrappedValueProperty(
    Func<T> getter,
    Action<T> setter)
  {
    _getter=getter;
    _setter=setter;
  }

  /// <inheritdoc/>
  public T GetValue()
  {
    return _getter();
  }

  /// <inheritdoc/>
  public bool ChangeValue(T value)
  {
    var old = _getter();
    if(EqualityComparer<T>.Default.Equals(old, value))
    {
      return false;
    }
    else
    {
      _setter(value);
      return true;
    }
  }
}

/// <summary>
/// This implements <see cref="IWrappedProperty{T}"/> for non-nullable reference
/// types, using a getter and setter delegate. <see cref="ChangeValue(T)"/> 
/// compares old and new values using Object.ReferenceEquals.
/// </summary>
/// <typeparam name="T"></typeparam>
public class WrappedInstanceProperty<T>: IWrappedProperty<T>
  where T : class
{
  private readonly Func<T> _getter;
  private readonly Action<T> _setter;

  public WrappedInstanceProperty(
    Func<T> getter,
    Action<T> setter)
  {
    _getter=getter;
    _setter=setter;
  }

  /// <inheritdoc/>
  public T GetValue()
  {
    return _getter();
  }

  /// <inheritdoc/>
  public bool ChangeValue(T value)
  {
    var old = _getter();
    if(Object.ReferenceEquals(old, value))
    {
      return false;
    }
    else
    {
      _setter(value);
      return true;
    }
  }
}

/// <summary>
/// This implements <see cref="IWrappedProperty{T}"/> for non-nullable reference
/// types, using a getter and setter delegate. <see cref="ChangeValue(T)"/> 
/// compares old and new values using Object.ReferenceEquals.
/// </summary>
/// <typeparam name="T"></typeparam>
public class WrappedNullableInstanceProperty<T>: IWrappedProperty<T?>
  where T : class?
{
  private readonly Func<T?> _getter;
  private readonly Action<T?> _setter;

  public WrappedNullableInstanceProperty(
    Func<T?> getter,
    Action<T?> setter)
  {
    _getter=getter;
    _setter=setter;
  }

  /// <inheritdoc/>
  public T? GetValue()
  {
    return _getter();
  }

  /// <inheritdoc/>
  public bool ChangeValue(T? value)
  {
    var old = _getter();
    if(Object.ReferenceEquals(old, value))
    {
      return false;
    }
    else
    {
      _setter(value);
      return true;
    }
  }
}

