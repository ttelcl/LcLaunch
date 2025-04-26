/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using ControlzEx.Theming;

using LcLauncher.IconUpdates;
using LcLauncher.Main.Rack.Tile;
using LcLauncher.Models;
using LcLauncher.Persistence;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class ShelfViewModel:
  ViewModelBase, IIconLoadJobSource, IPersisted, ITileListOwner, IHasTheme
{
  public ShelfViewModel(
    RackViewModel rack,
    ShelfModel model)
  {
    Rack = rack;
    ClaimTracker = Rack.Model.GetClaimTracker(model.Id);
    Store = Rack.Store;
    Model = model;
    _model = Model;
    SetThemeCommand = new DelegateCommand(
      p => Theme = (p as string) ?? "Olive");
    ToggleExpandedCommand = new DelegateCommand(
      p => IsExpanded = !IsExpanded);
    PrimaryTiles = new TileListViewModel(
      Rack.IconLoadQueue,
      this,
      model.PrimaryTiles);
    EnqueueIconJobs = new DelegateCommand(
      p => QueueIcons(false));
    RefreshIconJobs = new DelegateCommand(
      p => QueueIcons(true));
    ToggleCutCommand = new DelegateCommand(
      p => {
        IsKeyShelf = Rack.KeyShelf != this;
      });
    MoveMarkedShelfHereCommand = new DelegateCommand(
      p => MoveMarkedShelfHere(),
      p => CanMoveMarkedShelfHere());
    CreateNewShelfHereCommand = new DelegateCommand(
      p => CreateNewShelfHere(),
      p => Rack.KeyShelf == null && Rack.KeyTile == null);
    DeleteShelfCommand = new DelegateCommand(
      p => DeleteShelf(),
      p => CanDeleteShelf());
    EditCommand = new DelegateCommand(
      p => ShelfEditViewModel.Show(this),
      p => Rack.KeyShelf == null && Rack.KeyTile == null);
  }

  public ICommand SetThemeCommand { get; }

  public ICommand ToggleExpandedCommand { get; }

  public ICommand EnqueueIconJobs { get; }

  public ICommand RefreshIconJobs { get; }

  public ICommand ToggleCutCommand { get; }

  public ICommand MoveMarkedShelfHereCommand { get; }

  public ICommand CreateNewShelfHereCommand { get; }

  public ICommand DeleteShelfCommand { get; }

  public ICommand EditCommand { get; }

  public RackViewModel Rack { get; }

  public ShelfModel Model {
    get => _model;
    private set {
      if(_model != null)
      {
        // This property is 'initialize once'
        // Multi-init would violate TargetListModel invariant and probably
        // some more.
        throw new InvalidOperationException(
          "ShelfViewModel.Model: Can only be set once.");
      }
      if(SetInstanceProperty(ref _model!, value))
      {
        if(!this.ClaimTileList())
        {
          Trace.TraceWarning(
            $"ShelfViewModel.Model: Failed to claim tile list for "+
            $"'{TileListOwnerLabel}', already claimed by "+
            $"'{ClaimTracker.Owner?.TileListOwnerLabel ?? String.Empty}'");
        }
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

  public bool IsKeyShelf {
    get => _isKeyShelf;
    set {
      if(SetValueProperty(ref _isKeyShelf, value))
      {
        if(_isKeyShelf)
        {
          // Note: Make sure this doesn't recurse indefinitely
          // Setting Rack.KeyShelf to this will call this property
          // setter, but the 'ifs' above will block further recursion.
          Rack.KeyShelf = this;
        }
        else if(Rack.KeyShelf == this)
        {
          Rack.KeyShelf = null;
        } // else: don't affect Rack.KeyShelf
        RaisePropertyChanged(nameof(MarkShelfText));
      }
    }
  }
  private bool _isKeyShelf;

  public string MarkShelfText => IsKeyShelf ? "Unmark Shelf" : "Mark Shelf";

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
          Model.MarkDirty();
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
          Model.MarkDirty();
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
        if(Model.Shelf.Collapsed != !value)
        {
          Model.Shelf.Collapsed = !value;
          Model.MarkDirty();
        }
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

  public IEnumerable<IconLoadJob> GetIconLoadJobs(bool reload)
  {
    return PrimaryTiles.GetIconLoadJobs(reload);
  }

  public IconLoadQueue IconLoadQueue { get => Rack.IconLoadQueue; }

  private void QueueIcons(bool reload)
  {
    var before = IconLoadQueue.JobCount();
    this.EnqueueAllIconJobs(reload);
    var after = IconLoadQueue.JobCount();
    Trace.TraceInformation(
      $"Queued {after - before} icon load jobs ({after} - {before}) for {Model.Id}");
  }

  public bool IsDirty { get => Model.IsDirty; }

  public void MarkDirty()
  {
    Model.MarkDirty();
    RaisePropertyChanged(nameof(IsDirty));
  }

  public void SaveIfDirty()
  {
    if(IsDirty)
    {
      Trace.TraceInformation(
        $"Saving shelf {Model.Id}");
      // No need to 'rebuild' anything, since there are no sub-models
      Model.Save();
      RaisePropertyChanged(nameof(IsDirty));
    }
  }

  public string TileListOwnerLabel { get => $"Shelf {ShelfId}"; }
  public TileListOwnerTracker ClaimTracker { get; }
  public bool ClaimPriority { get => true; }

  internal void GatherTileLists(Dictionary<Guid, TileListViewModel> buffer)
  {
    PrimaryTiles.GatherTileLists(buffer);
  }

  private bool CanMoveMarkedShelfHere()
  {
    if(Rack.KeyShelf == null)
    {
      return false;
    }
    if(Rack.KeyShelf == this)
    {
      return false;
    }
    return true;
  }

  private void MoveMarkedShelfHere()
  {
    if(CanMoveMarkedShelfHere())
    {
      var keyShelf = Rack.KeyShelf!;
      var sourceLocation = Rack.GetShelfLocation(keyShelf);
      var destinationLocation = Rack.GetShelfLocation(this);
      if(sourceLocation != null && destinationLocation != null)
      {
        Rack.MoveShelf(
          sourceLocation.Value,
          destinationLocation.Value);
      }
    }
    Rack.KeyShelf = null;
  }

  private void CreateNewShelfHere()
  {
    var sourceLocation = Rack.GetShelfLocation(this);
    if(sourceLocation == null)
    {
      return;
    }
    var _ = Rack.CreateNewShelf(sourceLocation.Value, null, Theme);
    // Todo: open editor
  }

  private bool CanDeleteShelf()
  {
    if(Rack.KeyShelf != null || Rack.KeyTile != null)
    {
      return false;
    }
    if(SecondaryTiles != null)
    {
      return false;
    }
    return true;
  }

  private bool GetIsEmpty()
  {
    return PrimaryTiles.Tiles.All(t => t.IsEmpty);
  }

  private void DeleteShelf()
  {
    if(CanDeleteShelf())
    {
      if(!GetIsEmpty())
      {
        var response = MessageBox.Show(
          "This shelf is not empty. Do you really want to delete it?",
          "Delete Shelf",
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning);
        if(response != MessageBoxResult.Yes)
        {
          return;
        }
      }
      var sourceLocation = Rack.GetShelfLocation(this);
      if(sourceLocation != null)
      {
        // We are about to detach this shelf from the rack.
        // Make sure its persisted model is up to date (it may still
        // be in use by other racks!)
        SaveIfDirty();

        var columnVm = Rack.Columns[sourceLocation.Value.ColumnIndex];

        // Remove the shelf vm from the column
        columnVm.Shelves.RemoveAt(sourceLocation.Value.ShelfIndex);

        // Remove the shelf model from the column
        columnVm.Model.RemoveAt(sourceLocation.Value.ShelfIndex);

        Rack.MarkDirty();
        Rack.SaveIfDirty();

        // We don't delete the backing store content. The shelf
        // may be in use elsewhere still.
      }
    }
  }

  //
}
