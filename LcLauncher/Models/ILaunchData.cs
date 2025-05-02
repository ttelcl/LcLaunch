/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LcLauncher.Models;

/// <summary>
/// Temporary common interface for old and new launch data models.
/// </summary>
public interface ILaunchData
{
  string TargetPath { get; set; }
  string? Title { get; set; }
  string? Tooltip { get; set; }
  string? IconSource { get; set; }
  string? Icon48 { get; set; }
  string? Icon32 { get; set; }
  string? Icon16 { get; set; }
}

public interface IShellLaunchData: ILaunchData
{
  string Verb { get; set; }
}

public interface IRawLaunchData: ILaunchData
{
  string? WorkingDirectory { get; set; }
  List<string> Arguments { get; }
  Dictionary<string, string?> Environment { get; }
  Dictionary<string, PathEdit> PathEnvironment { get; }
}

