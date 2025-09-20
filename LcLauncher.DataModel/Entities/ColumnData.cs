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
/// Describes a V3 column (before V3 these were implicit and nameless).
/// Not serialized separately, but as part of <see cref="RackData"/>
/// </summary>
public class ColumnData
{
  /// <summary>
  /// Deserialization constructor
  /// </summary>
  public ColumnData(
    TickId id,
    IEnumerable<TickId> shelves,
    string? title)
  {
    Id = id;
    Title =
      String.IsNullOrEmpty(title)
      ? $"Column {id}"
      : title;
    Shelves = shelves.ToList();
  }

  /// <summary>
  /// The column Id. Currently not actively used.
  /// </summary>
  [JsonProperty("id")]
  public TickId Id { get; }

  /// <summary>
  /// The column title (new in V3)
  /// </summary>
  [JsonProperty("title")]
  public string Title { get; set; }

  /// <summary>
  /// The IDs of the shelves in this column, in their order
  /// of appearance from top to bottom.
  /// </summary>
  [JsonProperty("shelves")]
  public List<TickId> Shelves { get; }
}
