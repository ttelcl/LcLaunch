/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

public class GroupTileViewModel: TileViewModel
{
  public GroupTileViewModel(
    TileGroup model)
  {
    Model = model;
    ToggleGroupCommand = new DelegateCommand(
      p => IsActive = !IsActive,
      p => CanToggleActive);
  }

  public ICommand ToggleGroupCommand { get; }

  public TileGroup Model { get; }

  public string Title {
    get => Model.Title;
    set {
      if(value != Model.Title)
      {
        Model.Title = value;
        RaisePropertyChanged(nameof(Title));
        if(String.IsNullOrEmpty(Model.Tooltip))
        {
          RaisePropertyChanged(nameof(EffectiveTooltip));
        }
      }
    }
  }

  public string? Tooltip {
    get => Model.Tooltip;
    set {
      if(value != Model.Tooltip)
      {
        Model.Tooltip = value;
        RaisePropertyChanged(nameof(Tooltip));
        RaisePropertyChanged(nameof(EffectiveTooltip));
      }
    }
  }

  public string EffectiveTooltip {
    get {
      var tooltip = Model.Tooltip;
      if(String.IsNullOrEmpty(tooltip))
      {
        tooltip = Model.Title;
      }
      return tooltip;
    }
  }

  public override TileData? GetModel()
  {
    return TileData.GroupTile(Model);
  }

  public bool IsActive {
    get => _isActive;
    set {
      var wasActive = TrueActive;
      var newValue = value && CanToggleActive;
      if(SetValueProperty(ref _isActive, newValue))
      {
        if(Host == null)
        {
          // Implies CanToggleActive is false -> _isActive is false
          return;
        }
        if(!_isActive && wasActive)
        {
          Host.TileList.Shelf.ActiveSecondaryTile = null;
        }
        else if(_isActive && !wasActive)
        {
          Host.TileList.Shelf.ActiveSecondaryTile = this;
        }
      }
    }
  }
  private bool _isActive = false;

  public bool TrueActive {
    get {
      if(ChildTiles == null || Host == null)
      {
        return false;
      }
      else
      {
        return Host.TileList.Shelf.SecondaryTiles == ChildTiles;
      }
    }
  }

  public bool CanToggleActive {
    get => ChildTiles != null && Host != null;
  }

  public TileListViewModel? ChildTiles {
    get => _childTiles;
    set {
      if(SetNullableInstanceProperty(ref _childTiles, value))
      {
        RaisePropertyChanged(nameof(CanToggleActive));
        if(value == null)
        {
          IsActive = false;
        }
      }
    }
  }
  private TileListViewModel? _childTiles;

  public override void OnHostChanged(
    TileHostViewModel? oldHost, TileHostViewModel? newHost)
  {
    if(ChildTiles == null && newHost != null)
    {
      var shelf = newHost.TileList.Shelf;
      var store = shelf.Store;
      var tiles = TileListModel.Load(store, Model.TileList);
      if(tiles != null)
      {
        Trace.TraceInformation(
          "Loaded secondary tile list {0} (for tile '{1}')",
          Model.TileList, Model.Title);
        var vm = new TileListViewModel(shelf, tiles);
        ChildTiles = vm;
      }
      else
      {
        Trace.TraceError(
          "Failed to load secondary tile list {0} (for tile '{1}')",
          Model.TileList, Model.Title);
        ChildTiles = null;
      }
    }
    else if(newHost == null)
    {
      ChildTiles = null;
    }
  }
}
