/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Ttelcl.Persistence.API;

namespace LcLauncher.DataModel.Entities;

/// <summary>
/// The shelf data (serializable)
/// </summary>
public class ShelfData: IJsonStorable
{
  /// <summary>
  /// Deserialization constructor
  /// </summary>
  public ShelfData(
    string? title,
    bool collapsed = false,
    string? theme = null,
    TickId? id = null)
  {
    Title = title ?? "";
    Theme = theme;
    Collapsed = collapsed;

    Id = id ?? TickId.Zero;
  }

  /// <summary>
  /// The shelf Id (model V3)
  /// </summary>
  [JsonProperty("id")]
  public TickId Id { get; set; }

  /// <summary>
  /// The shelf title
  /// </summary>
  [JsonProperty("title")]
  public string Title { get; set; }

  /// <summary>
  /// The name of the color theme used for this shelf
  /// </summary>
  [JsonProperty("theme", NullValueHandling = NullValueHandling.Ignore)]
  public string? Theme { get; set; }

  /// <summary>
  /// Whether or not the shelf is in its collapsed state
  /// </summary>
  [JsonProperty("collapsed")]
  public bool Collapsed { get; set; }
}
