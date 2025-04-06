/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LcLauncher.Models;

/// <summary>
/// Description of RawLaunch
/// </summary>
public class RawLaunch: LaunchData
{
  /// <summary>
  /// Create a new RawLaunch
  /// </summary>
  public RawLaunch(
    string target,
    string? tooltip = null,
    string? title = null,
    ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
    string? iconSource = null,
    string? icon48 = null,
    string? icon32 = null,
    string? icon16 = null,
    string? workingDirectory = null,
    IEnumerable<string>? arguments = null,
    IDictionary<string, string?>? env = null,
    IDictionary<string, PathEdit>? pathenv = null)
    : base(target, tooltip, title, windowStyle,
           iconSource, icon48, icon32, icon16)
  {
    WorkingDirectory = workingDirectory;
    Arguments = new(arguments ?? []);
    Environment = new(env ?? new Dictionary<string, string?>());
    PathEnvironment = new(pathenv ?? new Dictionary<string, PathEdit>());
  }

  [JsonProperty("workingDirectory", NullValueHandling = NullValueHandling.Ignore)]
  public string? WorkingDirectory { get; set; }

  [JsonProperty("arguments")]
  public List<string> Arguments { get; }

  public bool ShouldSerializeArguments() => Arguments.Count > 0;

  [JsonProperty("env")]
  public Dictionary<string, string?> Environment { get; }

  public bool ShouldSerializeEnvironment() => Environment.Count > 0;

  [JsonProperty("pathenv")]
  public Dictionary<string, PathEdit> PathEnvironment { get; }

  public bool ShouldSerializePathEnvironment() => PathEnvironment.Count > 0;
}

public class PathEdit
{
  public PathEdit(
    IEnumerable<string>? prepend = null,
    IEnumerable<string>? append = null)
  {
    Prepend = new (prepend ?? []);
    Append = new (append ?? []);
  }

  [JsonProperty("prepend")]
  public List<string> Prepend { get; }

  public bool ShouldSerializePrepend() => Prepend.Count > 0;

  [JsonProperty("append")]
  public List<string> Append { get; }

  public bool ShouldSerializeAppend() => Append.Count > 0;
}
