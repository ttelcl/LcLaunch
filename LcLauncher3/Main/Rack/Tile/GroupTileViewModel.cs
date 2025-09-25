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

using LcLauncher.IconTools;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

using LcLauncher.DataModel.Entities;

namespace LcLauncher.Main.Rack.Tile;

public class GroupTileViewModel: TileViewModel, ICanQueueIcons /*, ITileListOwner*/
{
  public GroupTileViewModel(
    TileListViewModel ownerList,
    GroupData model)
    : base(ownerList)
  {
    PostIconLoadId = Guid.NewGuid();
    var rack = ownerList.Shelf.Rack.Model;
    //ClaimTracker = rack.GetClaimTracker(model.TileListId);
    TileListOwnerLabel =
      $"Group {PostIconLoadId} targeting {model.TileListId} from list {ownerList.Model.Id}";
    Model = model;
    ToggleGroupCommand = new DelegateCommand(
      p => IsActive = /*!IsConflicted
           && */ !IsActive
           && !GetIsKeyTile(),
      p => /*!IsConflicted &&*/ !GetIsKeyTile());
    ClickActionCommand = ToggleGroupCommand;
    //FixGraphCommand = new DelegateCommand(
    //  p => { ConditionalReplaceWithClone(); },
    //  p => IsConflicted);
    //EditGroupCommand = new DelegateCommand(
    //  p => EditGroup(),
    //  p => !IsConflicted && !GetIsKeyTile());
    ToggleCutCommand = new DelegateCommand(
      p => {
        if(Host != null)
        {
          Host.IsKeyTile = Host.Rack.KeyTile != Host && !IsActive;
        }
      },
      p => Host!=null && !IsActive);
    EnqueueIconJobsCommand = new DelegateCommand(
      p => QueueIcons(false));
    RefreshIconJobsCommand = new DelegateCommand(
      p => QueueIcons(true));
    GroupIcons = new ObservableCollection<GroupIconViewModel>();
    //var childModel = TileListModel.Load(
    //  ownerList.Shelf.Rack.Model, model.TileListId);
    var childModel = TileListModel.Load(rack, model.TileListId);
    if(childModel == null)
    {
      Trace.TraceWarning(
        $"Creating missing tile list {model.TileListId}");
      childModel = TileListModel.Create(
        rack, model.TileListId);
      childModel.MarkDirty();
    }
    ChildTiles = new TileListViewModel(
      //ownerList.Shelf.Rack.IconLoadQueue,
      ownerList.Shelf,
      childModel);
    // BEWARE: FIX THIS
    //ChildTiles.SaveIfDirty();
    ResetGroupIcons();
  }

  public ICommand ToggleCutCommand { get; }

  public ICommand ToggleGroupCommand { get; }

  //public ICommand EditGroupCommand { get; }

  //public ICommand FixGraphCommand { get; }

  public ICommand EnqueueIconJobsCommand { get; }

  public ICommand RefreshIconJobsCommand { get; }

  public Guid PostIconLoadId { get; }

  public override string PlainIcon { get => "DotsGrid"; }

  public GroupData Model { get; }

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
            // 'Unselect' it instead.
            Host.Rack.KeyTile = null;
          }
        }
        RaisePropertyChanged(nameof(ToggleGroupIcon));
        RaisePropertyChanged(nameof(ToggleGroupText));
      }
    }
  }
  private bool _isActive = false;

  public string ToggleGroupIcon {
    get => IsActive ? "ArchiveArrowUpOutline" : "ArchiveArrowDownOutline";
  }

  public string ToggleGroupText {
    get => IsActive ? "Hide Group" : "Show Group";
  }

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

  //private Guid ReplaceWithClone()
  //{
  //  var targetClone = ChildTiles.CreateClone();
  //  var modelClone = new Model2.GroupData(
  //    targetClone.Model.Id,
  //    Model.Title,
  //    Model.Tooltip);
  //  var clone = new GroupTileViewModel(
  //    OwnerList,
  //    modelClone);
  //  Host!.Tile = clone;
  //  OwnerList.MarkDirty();
  //  //OwnerList.SaveIfDirty();
  //  return targetClone.Model.Id;
  //}

  //private Guid ConditionalReplaceWithClone()
  //{
  //  if(IsConflicted)
  //  {
  //    return ReplaceWithClone();
  //  }
  //  return Model.TileListId;
  //}

  public string TileListOwnerLabel { get; }

  // public TileListOwnerTracker ClaimTracker { get; }

  public bool ClaimPriority { get => false; }

  // public bool IsConflicted { get => !this.OwnsTileList(); }

  //public void EditGroup()
  //{
  //  if(Host != null)
  //  {
  //    var editor = new GroupEditViewModel(Host); // automatically picks up 'this'
  //    editor.IsActive = true;
  //  }
  //}

  protected override void OnHostChanged(
    TileHostViewModel? oldHost, TileHostViewModel? newHost)
  {
    // Only Claim tile list while this is socketed in its host
    if(newHost != null)
    {
      //if(!this.ClaimTileList())
      //{
      //  Trace.TraceWarning(
      //    $"GroupTileViewModel: Failed to claim tile list for "+
      //    $"'{TileListOwnerLabel}', already claimed by "+
      //    $"'{ClaimTracker.Owner?.TileListOwnerLabel ?? String.Empty}'");
      //}
    }
    else
    {
      //// Release the claim when the host is removed
      //this.ReleaseTileList();
    }
  }

  public void QueueIcons(bool regenerate)
  {
    ChildTiles.QueueIcons(regenerate);
  }
}
