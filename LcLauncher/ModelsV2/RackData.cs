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

namespace LcLauncher.ModelsV2;

/// <summary>
/// The serializable Rack Content
/// </summary>
public class RackData
{
  public RackData(
    IEnumerable<List<Guid>> columns)
  {
    var columnList = columns.ToList();
    Columns = columnList;
    //if(columnList.Count != 3)
    //{
    //  throw new ArgumentException("RackData must have exactly 3 columns");
    //}
    // Code refactoring in progress. For now make the *Minimum* 3
    while(columnList.Count < 3)
    {
      columnList.Add([]);
    }
  }

  [JsonProperty("columns")]
  public IReadOnlyList<List<Guid>> Columns { get; }
}
