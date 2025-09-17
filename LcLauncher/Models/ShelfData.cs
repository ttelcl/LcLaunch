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

namespace LcLauncher.Models;

/// <summary>
/// The shelf data (serializable)
/// </summary>
public class ShelfData
{
  public ShelfData(
    string? title,
    bool collapsed = false,
    string? theme = null)
  {
    Title = title ?? "";
    Theme = theme;
    Collapsed = collapsed;
  }

  [JsonProperty("title")]
  public string Title { get; set; }

  [JsonProperty("theme", NullValueHandling = NullValueHandling.Ignore)]
  public string? Theme { get; set; }

  [JsonProperty("collapsed")]
  public bool Collapsed { get; set; }
}
