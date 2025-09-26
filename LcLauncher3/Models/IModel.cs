/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Ttelcl.Persistence.API;

namespace LcLauncher.Models;

/// <summary>
/// Formalizes the common features of the 'model' classes: they
/// wrap an entity that has a <see cref="TickId"/> as identifier,
/// and forward that ID.
/// </summary>
public interface IModel<TEntity>: IHasTickId
  where TEntity : class, IHasTickId
{
  /// <summary>
  /// The underlying entity (often an <see cref="IJsonStorable"/>
  /// itself, or embedded in one)
  /// </summary>
  TEntity Entity { get; }
}

public interface IRebuildableModel<TEntity>: IModel<TEntity>
  where TEntity : class, IHasTickId
{
  /// <summary>
  /// Reconstruct the <see cref="IModel{TEntity}"/>'s
  /// <see cref="IModel{TEntity}.Entity"/> to match latest edits,
  /// reading for saving to the persistence layer.
  /// </summary>
  void RebuildEntity();
}

