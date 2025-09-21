/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Newtonsoft.Json;
using Ttelcl.Persistence.API;

namespace LcLauncher.Models;

public class GroupData
{
  public GroupData(
    Guid tilelist,
    string? title = null,
    string? tooltip = null)
  {
    TileList = tilelist;
    Title = String.IsNullOrEmpty(title) ? "Untitled Group" : title;
    Tooltip = tooltip;
  }

  [JsonProperty("title")]
  public string Title { get; set; }

  /// <summary>
  /// The tiles list ID in the original model
  /// </summary>
  [JsonProperty("tilelist")]
  public Guid TileList { get; set; }

  [JsonProperty("tooltip", NullValueHandling = NullValueHandling.Ignore)]
  public string? Tooltip { get; set; }
}
