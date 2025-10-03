/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.Main.Rack.Editors;

/// <summary>
/// Helps ensuring that at most one 'sub editor' is in edit mode
/// </summary>
public interface ISubEditor
{
  bool IsEditing { get; set; }

  string SubEditorName { get; }
}

public interface ISubEditorHost
{
  ISubEditor? CurrentSubEditor { get; set; }
}

