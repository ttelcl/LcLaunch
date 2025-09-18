/*
 * (c) 2025  ttelcl / ttelcl
 */

using System.Collections.Generic;

using Newtonsoft.Json;

namespace LcLauncher.DataModel.Entities;

/// <summary>
/// Describes edits to PATH-like environment variables.
/// </summary>
public class PathEdit
{
  /// <summary>
  /// Deserialization constructor
  /// </summary>
  public PathEdit(
    IEnumerable<string>? prepend = null,
    IEnumerable<string>? append = null)
  {
    Prepend = new (prepend ?? []);
    Append = new (append ?? []);
  }

  /// <summary>
  /// Items to prepend
  /// </summary>
  [JsonProperty("prepend")]
  public List<string> Prepend { get; }

  /// <summary>
  /// Whether <see cref="Prepend"/> should be serialized. This hides
  /// the property when it is empty.
  /// </summary>
  public bool ShouldSerializePrepend() => Prepend.Count > 0;

  /// <summary>
  /// Items to append
  /// </summary>
  [JsonProperty("append")]
  public List<string> Append { get; }

  /// <summary>
  /// Whether <see cref="Append"/> should be serialized. This hides
  /// the property when it is empty.
  /// </summary>
  public bool ShouldSerializeAppend() => Append.Count > 0;
}
