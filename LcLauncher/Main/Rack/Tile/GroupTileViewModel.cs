/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using LcLauncher.IconUpdates;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

public class GroupTileViewModel: TileViewModel, IPostIconLoadActor
{
  public GroupTileViewModel(
    TileListViewModel ownerList,
    TileGroup model)
    : base(ownerList)
  {
    PostIconLoadId = Guid.NewGuid();
    Model = model;
    ToggleGroupCommand = new DelegateCommand(
      p => IsActive = !IsActive);
    GroupIcons = new ObservableCollection<GroupIconViewModel>();
    var childModel = TileListModel.Load(ownerList.Shelf.Store, model.TileList);
    if(childModel == null)
    {
      Trace.TraceWarning(
        $"Creating missing tile list {model.TileList}");
      childModel = TileListModel.Create(
        ownerList.Shelf.Store,
        model.TileList);
    }
    ChildTiles = new TileListViewModel(
      ownerList.Shelf.Rack.IconLoadQueue,
      ownerList.Shelf,
      childModel);
    ResetGroupIcons();
  }

  public ICommand ToggleGroupCommand { get; }

  public Guid PostIconLoadId { get; }

  public override string PlainIcon { get => "DotsGrid"; }

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
      var wasActive = OwnerList.Shelf.ActiveSecondaryTile == this;
      if(SetValueProperty(ref _isActive, value))
      {
        if(!wasActive && _isActive)
        {
          // activate this group
          OwnerList.Shelf.ActiveSecondaryTile = this;
        }
        else if(wasActive && !_isActive)
        {
          // deactivate this group
          OwnerList.Shelf.ActiveSecondaryTile = null;
        }
      }
    }
  }
  private bool _isActive = false;

  public TileListViewModel ChildTiles { get; }

  public ObservableCollection<GroupIconViewModel> GroupIcons { get; }

  public void ResetGroupIcons()
  {
    GroupIcons.Clear();
    foreach(var tvm in ChildTiles.Tiles.Take(16))
    {
      var givm = new GroupIconViewModel(this, tvm.Tile);
      GroupIcons.Add(givm);
    }
  }

  public override IEnumerable<IconLoadJob> GetIconLoadJobs(bool reload)
  {
    foreach(var job in ChildTiles.GetIconLoadJobs(reload))
    {
      yield return job;
    }
    ChildTiles.IconJobQueue.QueuePostLoadActor(this);
  }

  public void PostIconLoad()
  {
    Trace.TraceInformation(
      $"PostIconLoad for group tile '{Title}'");
    ResetGroupIcons();
  }
}
