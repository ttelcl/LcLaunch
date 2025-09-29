/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LcLauncher.Main.Rack;

public struct ShelfLocation
{
  public ShelfLocation(
    ColumnViewModel column,
    int shelfIndex)
  {
    Column = column;
    ShelfIndex = shelfIndex;
  }

  public ColumnViewModel Column { get; }

  public int ShelfIndex { get; }

}
