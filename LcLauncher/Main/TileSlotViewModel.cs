﻿/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using LcLauncher.WpfUtilities;
using System.Windows.Input;

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

    // Temporary test content
    TestSecondaryCommand = new DelegateCommand(
      p => {
        if(Owner.IsPrimary)
        {
          Owner.Owner.ToggleSecondaryContent(_secondaryContent);
        }
      });
    if(Owner.IsPrimary)
    {
      _secondaryContent = new ShelfContentViewModel(Owner.Owner, false);
    }
  }

  public ShelfContentViewModel Owner { get; }

  public int Position { get; }

  public int Row { get => Position / 4; }

  public int Column { get => Position % 4; }

  public object? /* placeholder type */ Content {
    get => _content;
    set {
      if(SetNullableInstanceProperty(ref _content, value))
      {
        IsEmpty = value == null;
      }
    }
  }
  private object? _content;

  public bool IsEmpty {
    get => _isEmpty;
    private set {
      if(SetValueProperty(ref _isEmpty, value))
      {
      }
    }
  }
  private bool _isEmpty = true;

  /* Temporary test content */
  public ICommand TestSecondaryCommand { get; }
  private ShelfContentViewModel? _secondaryContent;
}
