/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main;

/// <summary>
/// A slot to hold a tile, at a *fixed* position in a
/// <see cref="ShelfContentViewModel"/>. This is the state
/// to render the outside chrome of the tile. The functionality
/// and inner rendering is plugged in this slot.
/// </summary>
public class TileSlotViewModel: ViewModelBase
{
  public TileSlotViewModel(
    ShelfContentViewModel owner,
    int position)
  {
    Owner = owner;
    Position = position;
  }

  public ShelfContentViewModel Owner { get; }

  public int Position { get; }

  public int Row { get => Position / 4; }

  public int Column { get => Position % 4; }

}
