/*
 * (c) 2025  ttelcl / ttelcl
 */

using System.Collections.Generic;

using Newtonsoft.Json;

namespace LcLauncher.Models;

/// <summary>
/// Describes edits to PATH-like environment variables.
/// </summary>
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
