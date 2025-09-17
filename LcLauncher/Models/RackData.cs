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

namespace LcLauncher.Models;

/// <summary>
/// The serializable Rack Content
/// </summary>
public class RackData
{
  public RackData(
    IEnumerable<List<Guid>> columns,
    IEnumerable<List<TickId>>? columns2 = null)
  {
    var columnList = columns.ToList();
    Columns = columnList;
    //if(columnList.Count != 3)
    //{
    //  throw new ArgumentException("RackData must have exactly 3 columns");
    //}
    // Code refactoring in progress. For now make the *Minimum* 3
    var columns2List = columns2 != null ? columns2.ToList() : [];
    while(columnList.Count < 3 && columnList.Count < columns2List.Count)
    {
      columnList.Add([]);
    }
    while(columns2List.Count < columnList.Count)
    {
      columns2List.Add([]);
    }
    Columns2 = columns2List;
    Upgrading = columns2 == null;
  }

  [JsonProperty("columns")]
  public IReadOnlyList<List<Guid>> Columns { get; }

  [JsonProperty("columns2")]
  public IReadOnlyList<List<TickId>> Columns2 { get; }

  [JsonIgnore]
  public bool Upgrading { get; }
}
