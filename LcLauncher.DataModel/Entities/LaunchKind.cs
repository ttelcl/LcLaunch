/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LcLauncher.DataModel.Entities;

/// <summary>
/// Different ways to launch an executable
/// </summary>
public enum LaunchKind
{
  /// <summary>
  /// Not recognized as a valid target
  /// </summary>
  Invalid,

  /// <summary>
  /// A plain executable target, with full control over arguments
  /// and environment. Target must be a full path to a *.exe file.
  /// </summary>
  Raw,

  /// <summary>
  /// A document target to be launched in shell mode. Target can
  /// be (pretty much) any file. Even an executable, but raw mode
  /// would offere more flexibility.
  /// </summary>
  Document,

  /// <summary>
  /// An application identifier that can be resolved through
  /// shell:AppsFolder\{AppId}. Could be an executable, but typically
  /// this is a UWP app. Optionally has a prefix "shell:AppsFolder\"
  /// (if missing that will be added).
  /// </summary>
  ShellApplication,

  /// <summary>
  /// Some URI other than a shell app. E.g. https://... or
  /// onenote:...
  /// </summary>
  UriKind,

  ///// <summary>
  ///// Looks like a valid absolute file path, but it is missing
  ///// </summary>
  //Missing,
}
