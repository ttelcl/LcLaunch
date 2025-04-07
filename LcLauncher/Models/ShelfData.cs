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

public class ShelfData0
{
  public ShelfData0(
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

  public static ShelfData0 LoadFile(string path)
  {
    return JsonConvert.DeserializeObject<ShelfData0>(File.ReadAllText(path))
      ?? new ShelfData0("Bad Shelf File", []);
  }
}
