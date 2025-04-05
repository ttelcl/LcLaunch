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

public class TileGroup0
{
  public TileGroup0(
    string? title,
    IEnumerable<TileData0?>? tiles = null)
  {
    Title = String.IsNullOrEmpty(title) ? "Untitled Group" : title;
    Tiles = tiles?.ToList() ?? [];
  }

  [JsonProperty("tiles")]
  public List<TileData0?> Tiles { get; }

  [JsonProperty("title")]
  public string Title { get; set; }

}
