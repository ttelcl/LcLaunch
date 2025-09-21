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

#if MODEL2

/// <summary>
/// (To be deprecated!) Old launch model specific for non-shell launcher
/// </summary>
public class RawLaunch: LaunchDataBase, IRawLaunchData
{
  /// <summary>
  /// Create a new RawLaunch
  /// </summary>
  public RawLaunch(
    string target,
    string? title = null,
    string? tooltip = null,
    ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal,
    string? iconSource = null,
    string? icon48 = null,
    string? icon32 = null,
    string? icon16 = null,
    string? workingDirectory = null,
    IEnumerable<string>? arguments = null,
    IDictionary<string, string?>? env = null,
    IDictionary<string, PathEdit>? pathenv = null)
    : base(target, title, tooltip, windowStyle,
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

  [JsonIgnore]
  public override bool ShellMode => false;
}

#endif
