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

namespace LcLauncher.Models;

public class RackData
{
  public RackData(
    IEnumerable<List<Guid>> columns)
  {
    Columns = columns.ToList();
    if(Columns.Count != 3)
    {
      throw new ArgumentException("RackData must have exactly 3 columns");
    }
  }

  [JsonProperty("columns")]
  public IReadOnlyList<List<Guid>> Columns { get; }

}
