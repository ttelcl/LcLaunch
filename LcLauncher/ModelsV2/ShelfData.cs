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

namespace LcLauncher.ModelsV2;

/// <summary>
/// The shelf data (serializable)
/// </summary>
public class ShelfData
{
  public ShelfData(
    string? title,
    bool collapsed = false,
    string? theme = null,
    Guid? guid = null,
    TickId? tid = null)
  {
    Title = title ?? "";
    Theme = theme;
    Collapsed = collapsed;

    IdOld = guid ?? Guid.Empty;
    Tid = tid ?? TickId.Zero;
  }

  /// <summary>
  /// The new Id
  /// </summary>
  [JsonProperty("tid")]
  public TickId Tid { get; set; }

  /// <summary>
  /// Backward compatibility Guid. The external ID is authorative, this
  /// is merely a copy for clarity.
  /// </summary>
  [JsonProperty("guid")]
  public Guid IdOld { get; set; }

  [JsonProperty("title")]
  public string Title { get; set; }

  [JsonProperty("theme", NullValueHandling = NullValueHandling.Ignore)]
  public string? Theme { get; set; }

  [JsonProperty("collapsed")]
  public bool Collapsed { get; set; }
}
