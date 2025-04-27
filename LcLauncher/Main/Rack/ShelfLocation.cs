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
    int columnIndex,
    int shelfIndex)
  {
    ColumnIndex = columnIndex;
    ShelfIndex = shelfIndex;
  }

  public int ColumnIndex { get; }

  public int ShelfIndex { get; }

}
