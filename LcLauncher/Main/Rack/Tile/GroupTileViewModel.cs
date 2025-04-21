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

public class GroupTileViewModel: TileViewModel, IPostIconLoadActor, ITileListOwner
{
  public GroupTileViewModel(
    TileListViewModel ownerList,
    TileGroup model)
    : base(ownerList)
  {
    PostIconLoadId = Guid.NewGuid();
    var rack = ownerList.Shelf.Rack.Model;
    ClaimTracker = rack.GetClaimTracker(model.TileList);
    TileListOwnerLabel =
      $"Group {PostIconLoadId} targeting {model.TileList} from list {ownerList.Model.Id}";
    Model = model;
    ToggleGroupCommand = new DelegateCommand(
      p => IsActive = !IsConflicted && !IsActive);
    FixGraphCommand = new DelegateCommand(
      p => { ConditionalReplaceWithClone(); },
      p => IsConflicted);
    ToggleCutCommand = new DelegateCommand(
      p => {
        if(Host != null)
        {
          Host.IsKeyTile = Host.Rack.KeyTile != Host && !IsActive;
        }
      },
      p => Host!=null && !IsActive);
    GroupIcons = new ObservableCollection<GroupIconViewModel>();
    var childModel = TileListModel.Load(ownerList.Shelf.Rack.Model, model.TileList);
    if(childModel == null)
    {
      Trace.TraceWarning(
        $"Creating missing tile list {model.TileList}");
      childModel = TileListModel.Create(
        ownerList.Shelf.Rack.Model,
        model.TileList);
      childModel.MarkDirty();
    }
    ChildTiles = new TileListViewModel(
      ownerList.Shelf.Rack.IconLoadQueue,
      ownerList.Shelf,
      childModel);
    ChildTiles.SaveIfDirty();
    if(!this.ClaimTileList())
    {
      Trace.TraceWarning(
        $"GroupTileViewModel: Failed to claim tile list for "+
        $"'{TileListOwnerLabel}', already claimed by "+
        $"'{ClaimTracker.Owner?.TileListOwnerLabel ?? String.Empty}'");
    }
    ResetGroupIcons();
  }

  public ICommand ToggleCutCommand { get; }

  public ICommand ToggleGroupCommand { get; }

  public ICommand FixGraphCommand { get; }

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
    // Note: IsActive and Host.IsKeyTile cannot both be true
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
        if(IsActive)
        {
          if(Host != null)
          {
            Host.IsKeyTile = false;
          }
        }
        else
        {
          if(Host != null && ChildTiles.ContainsKeyTile())
          {
            // Make sure the key tile does not go invisible
            // 'Uncut' it instead.
            Host.Rack.KeyTile = null;
          }
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

  private Guid ReplaceWithClone()
  {
    var targetClone = ChildTiles.CreateClone();
    var modelClone = new TileGroup(
      targetClone.Model.Id,
      Model.Title,
      Model.Tooltip);
    var clone = new GroupTileViewModel(
      OwnerList,
      modelClone);
    Host!.Tile = clone;
    OwnerList.MarkDirty();
    //OwnerList.SaveIfDirty();
    return targetClone.Model.Id;
  }

  private Guid ConditionalReplaceWithClone()
  {
    if(IsConflicted)
    {
      return ReplaceWithClone();
    }
    return Model.TileList;
  }

  public string TileListOwnerLabel { get; }
  public TileListOwnerTracker ClaimTracker { get; }
  public bool ClaimPriority { get => false; }
  public bool IsConflicted { get => !this.OwnsTileList(); }
}
