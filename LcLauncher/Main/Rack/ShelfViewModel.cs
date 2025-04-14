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

using ControlzEx.Theming;

using LcLauncher.Main.Rack.Tile;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

using GroupTileViewModel = LcLauncher.Main.Rack.Tile.GroupTileViewModel;

namespace LcLauncher.Main.Rack;

public class ShelfViewModel: ViewModelBase
{
  public ShelfViewModel(
    ColumnViewModel column,
    ShelfModel model)
  {
    Column = column;
    Store = column.Rack.Store;
    Model = model;
    _model = Model;
    SetThemeCommand = new DelegateCommand(
      p => Theme = (p as string) ?? "Olive");
    ToggleExpandedCommand = new DelegateCommand(
      p => IsExpanded = !IsExpanded);
    PrimaryTiles = new TileListViewModel(this, model.PrimaryTiles);
  }

  public ICommand SetThemeCommand { get; }

  public ICommand ToggleExpandedCommand { get; }

  public ColumnViewModel Column { get; }

  public RackViewModel Rack => Column.Rack;

  public ShelfModel Model { 
    get => _model;
    private set {
      if(SetValueProperty(ref _model, value))
      {
        Title = value.Shelf.Title;
        Theme = value.Shelf.Theme ?? "Olive";
        IsExpanded = !value.Shelf.Collapsed;
        ActiveSecondaryTile = null;
        RaisePropertyChanged(nameof(PrimaryTiles));
        RaisePropertyChanged(nameof(ShelfId));
      }
    }
  }
  private ShelfModel _model;

  public ILcLaunchStore Store { get; }

  public TileListViewModel PrimaryTiles { get; }

  public Guid ShelfId => Model.Id;

  public TileListViewModel? SecondaryTiles {
    get => _secondaryTiles;
    private set {
      if(SetNullableInstanceProperty(ref _secondaryTiles, value))
      {
        RaisePropertyChanged(nameof(HasSecondaryTiles));
      }
    }
  }
  private TileListViewModel? _secondaryTiles;

  public GroupTileViewModel? ActiveSecondaryTile {
    get => _groupTileViewModel;
    set {
      var oldGroup = _groupTileViewModel;
      if(SetNullableInstanceProperty(ref _groupTileViewModel, value))
      {
        SecondaryTiles = null;
        if(oldGroup != null)
        {
          oldGroup.IsActive = false;
        }
        if(_groupTileViewModel != null)
        {
          SecondaryTiles = _groupTileViewModel.ChildTiles;
        }
      }
    }
  }
  private GroupTileViewModel? _groupTileViewModel;

  public bool HasSecondaryTiles {
    get => _secondaryTiles != null;
  }

  public string Theme {
    get => _theme;
    set {
      if(SetValueProperty(ref _theme, value))
      {
        if(Model.Shelf.Theme != value)
        {
          Model.Shelf.Theme = value;
        }
        SetTheme("Dark." + value);
      }
    }
  }
  private string _theme = "Olive";

  public string Title {
    get => _title;
    set {
      if(SetValueProperty(ref _title, value))
      {
        if(Model.Shelf.Title != value)
        {
          Model.Shelf.Title = value;
        }
      }
    }
  }
  private string _title = "Shelf Title";

  public bool IsExpanded {
    get => _isExpanded;
    set {
      if(SetValueProperty(ref _isExpanded, value))
      {
        RaisePropertyChanged(nameof(ShelfExpandedIcon));
      }
    }
  }
  private bool _isExpanded = true;

  public string ShelfExpandedIcon =>
    IsExpanded ? "ChevronUpCircleOutline" : "ChevronDownCircleOutline";

  private ShelfView? Host { get; set; }

  internal void UpdateHost(ShelfView? host)
  {
    if(host == null)
    {
      Trace.TraceInformation(
        "ShelfViewModel.UpdateHost: Clearing host control");
      Host = null;
    }
    else
    {
      Trace.TraceInformation(
        "ShelfViewModel.UpdateHost: Setting host control");
      Host = host;
      SetTheme("Dark." + Theme);
    }
  }

  private void SetTheme(string? theme)
  {
    if(Host == null)
    {
      //Trace.TraceWarning("SetTheme: Host control is null");
      return;
    }
    if(string.IsNullOrEmpty(theme))
    {
      Trace.TraceWarning("SetTheme: Theme is null or empty");
      return;
    }
    ThemeManager.Current.ChangeTheme(Host, theme);
  }
}
