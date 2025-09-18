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

using Ttelcl.Persistence.API;

namespace LcLauncher.DataModel.Entities;

/// <summary>
/// Data for a group tile
/// </summary>
public class GroupData
{
  /// <summary>
  /// Deserialization constructor
  /// </summary>
  /// <param name="tilelist"></param>
  /// <param name="title"></param>
  /// <param name="tooltip"></param>
  public GroupData(
    TickId tilelist,
    string? title = null,
    string? tooltip = null)
  {
    TileListId = tilelist;
    Title = String.IsNullOrEmpty(title) ? "Untitled Group" : title;
    Tooltip = tooltip;
  }

  /// <summary>
  /// The group's title
  /// </summary>
  [JsonProperty("title")]
  public string Title { get; set; }

  /// <summary>
  /// The tiles list ID (pointing to a <see cref="TileListData"/> in the store)
  /// </summary>
  [JsonProperty("tilelist")]
  public TickId TileListId { get; set; }

  /// <summary>
  /// The tooltip for the tile
  /// </summary>
  [JsonProperty("tooltip", NullValueHandling = NullValueHandling.Ignore)]
  public string? Tooltip { get; set; }
}

