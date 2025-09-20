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
/// The serializable Rack Content (V3 - now with a variable
/// number of columns)
/// </summary>
public class RackData: IJsonStorable
{
  /// <summary>
  /// Deserializing constructor
  /// </summary>
  /// <param name="id"></param>
  /// <param name="rackName"></param>
  /// <param name="columns"></param>
  public RackData(
    TickId id,
    string rackName,
    IEnumerable<ColumnData> columns)
  {
    Id = id;
    RackName = rackName;
    Columns = columns.ToList();
    if(Columns.Count == 0)
    {
      Columns.Add(new ColumnData(TickId.New(), [], "Untitled Column"));
    }
  }

  /// <summary>
  /// The rack Id.
  /// </summary>
  [JsonProperty("id")]
  public TickId Id { get; }

  /// <summary>
  /// The name of the rack
  /// </summary>
  [JsonProperty("rackname")]
  public string RackName { get; set; }

  /// <summary>
  /// The columns in this rack
  /// </summary>
  [JsonProperty("columns")]
  public List<ColumnData> Columns { get; }
}
