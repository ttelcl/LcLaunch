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

/// <summary>
/// An <see cref="IModel{TEntity}"/> that has all the information
/// to rebuild its wrapped <typeparamref name="TEntity"/>. Note that
/// in some cases the Model does not have enough information for this,
/// and the rebuilding needs to be done at VM level, using
/// <see cref="IRebuildableWrapModel{TModel, TEntity}"/>.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
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

/// <summary>
/// A viewmodel wrapping a <see cref="IModel{TEntity}"/> which
/// in turn wraps a <typeparamref name="TEntity"/>
/// </summary>
/// <typeparam name="TModel">
/// The 'model' class augmenting the entity
/// </typeparam>
/// <typeparam name="TEntity">
/// The raw entity class that is stored in the storage.
/// </typeparam>
public interface IWrapsModel<TModel, TEntity>
  where TEntity : class, IHasTickId
  where TModel : class, IModel<TEntity>
{
}

/// <summary>
/// A <see cref="IWrapsModel{TModel, TEntity}"/> that can rebuild 
/// its inner entity. Implemented when that cannot be done by the
/// model <see cref="IModel{TEntity}"/> alone.
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public interface IRebuildableWrapModel<TModel, TEntity>: IWrapsModel<TModel, TEntity>
  where TEntity : class, IHasTickId
  where TModel : class, IModel<TEntity>
{
  /// <summary>
  /// Reconstruct the <see cref="IModel{TEntity}"/>'s
  /// <see cref="IModel{TEntity}.Entity"/> to match latest edits,
  /// reading for saving to the persistence layer.
  /// </summary>
  void RebuildEntity();
}

