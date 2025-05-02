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
  string TargetPath { get; }
  string? Title { get; }
  string? Tooltip { get; }
  string? IconSource { get; }
  string? Icon48 { get; }
  string? Icon32 { get; }
  string? Icon16 { get; }
}

public interface IShellLaunchData: ILaunchData
{
  string Verb { get; }
}

public interface IRawLaunchData: ILaunchData
{
  string? WorkingDirectory { get; }
  List<string> Arguments { get; }
  Dictionary<string, string?> Environment { get; }
  Dictionary<string, PathEdit> PathEnvironment { get; }
}

