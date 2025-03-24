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

public class ShelfData
{
  public ShelfData(
    string? title,
    IEnumerable<TileData?> tiles,
    string? theme = null)
  {
    Title = String.IsNullOrEmpty(title) ? "Untitled Shelf" : title;
    Tiles = tiles.ToList();
    Theme = theme;
  }

  [JsonProperty("title")]
  public string Title { get; set; }

  [JsonProperty("tiles")]
  public List<TileData?> Tiles { get; }

  [JsonProperty("theme", NullValueHandling = NullValueHandling.Ignore)]
  public string? Theme { get; set; }

}
