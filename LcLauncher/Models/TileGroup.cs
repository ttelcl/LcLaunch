﻿/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Newtonsoft.Json;

namespace LcLauncher.Models;

public class TileGroup
{
  public TileGroup(
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

  [JsonProperty("tilelist")]
  public Guid TileList { get; set; }

  [JsonProperty("tooltip", NullValueHandling = NullValueHandling.Ignore)]
  public string? Tooltip { get; set; }
}
