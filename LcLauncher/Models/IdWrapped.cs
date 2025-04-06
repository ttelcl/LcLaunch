/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LcLauncher.Models;

/// <summary>
/// A 'container' with precisely one item together with an ID.
/// </summary>
public class IdWrapped<T>
  where T: class
{
  /// <summary>
  /// Create a new IdWrapped
  /// </summary>
  public IdWrapped(
    Guid id,
    T item)
  {
    Id = id;
    Item = item;
  }

  public Guid Id { get; }

  public T Item { get; }
}
