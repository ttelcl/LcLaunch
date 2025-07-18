﻿/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAPICodePack.Shell;

namespace LcLauncher.ShellApps;

/// <summary>
/// A classification of shell apps for LcLauncher
/// (these are not universally agreed kinds)
/// </summary>
public enum ShellAppKind
{
  /// <summary>
  /// No attempt was made yet to classify the parsing name
  /// </summary>
  Unclassified,

  /// <summary>
  /// UWP / modern app / windows store app ID
  /// </summary>
  AppId,

  /// <summary>
  /// The parsing name is simply an existing file name
  /// </summary>
  PlainFileApp,

  /// <summary>
  /// The parsing name is simply an existing folder name
  /// </summary>
  PlainFolderApp,

  /// <summary>
  /// An existing file, but specified relative to a Folder GUID.
  /// Example "{6D809377-6AF0-444B-8957-A3773F02200E}\Git\cmd\git-gui.exe"
  /// (see <see cref="KnownFolders.All"/>)
  /// </summary>
  FolderFileApp,

  /// <summary>
  /// The parsing name is a dotted name (probably a classical COM identifier)
  /// </summary>
  DottedName,

  /// <summary>
  /// The parsing name is some kind of URI, be it an URL or something
  /// more custom
  /// </summary>
  UriKind,

  /// <summary>
  /// The parsing name is generated by windows. These seem to be aliases
  /// for *.lnk files. Name is like
  /// "Microsoft.AutoGenerated.{09087536-09B8-650C-5F13-0802BC517BA8}" (with
  /// the GUID part 'random')
  /// </summary>
  Autogenerated,

  /// <summary>
  /// Some unrecognized kind, often full of random symbol characters.
  /// Example "o{c0ntm~{8o+uIfi`Grk>i+w}_{[wS?.I8qrd6X1^"
  /// </summary>
  Other,
}
