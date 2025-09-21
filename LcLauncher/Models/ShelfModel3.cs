/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.DataModel.Entities;

namespace LcLauncher.Models;

public class ShelfModel3
{
  internal ShelfModel3(
    ColumnModel3 column,
    ShelfData entity)
  {
    Column = column;
    Entity = entity;
  }

  public ColumnModel3 Column { get; }

  public RackModel3 Rack => Column.Rack;

  public ShelfData Entity { get; }

}
