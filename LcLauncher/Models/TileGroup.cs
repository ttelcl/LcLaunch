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

namespace LcLauncher.Models;

public class TileGroup
{
  public TileGroup(
    string? title,
    IEnumerable<TileData?>? tiles = null)
  {
    Title = String.IsNullOrEmpty(title) ? "Untitled Group" : title;
    Tiles = tiles?.ToList() ?? [];
  }

  [JsonProperty("tiles")]
  public List<TileData?> Tiles { get; }

  [JsonProperty("title")]
  public string Title { get; set; }

}
