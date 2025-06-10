/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcLauncher.Main.AppPicker;

/// <summary>
/// Different kinds of launch tiles
/// </summary>
public enum TileKind
{
  /// <summary>
  /// Launched through an AppsFolder shortcut.
  /// The only requirement is that the target must be known in the
  /// AppsFolder
  /// </summary>
  ModernAppTile,

  /// <summary>
  /// A raw executable file. Allows for configuraing arguments and
  /// environment variables
  /// </summary>
  ExecutableTile,

  /// <summary>
  /// Any existing file or folder
  /// </summary>
  DocumentTile,

  /// <summary>
  /// A valid URI. Does not require AppsFolder registration nor require
  /// being an existing file or folder
  /// </summary>
  UriTile,

}
