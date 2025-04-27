/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.IconUpdates;

/// <summary>
/// An item that has some work to do after icon loading is complete.
/// </summary>
public interface IPostIconLoadActor
{
  /// <summary>
  /// Uniquely identifies this actor, aiding in avoiding
  /// duplicate calls.
  /// </summary>
  Guid PostIconLoadId { get; }

  /// <summary>
  /// Calback to be called after icon loading is complete.
  /// </summary>
  void PostIconLoad();
}

