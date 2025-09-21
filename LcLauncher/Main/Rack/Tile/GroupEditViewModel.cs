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

using LcLauncher.Models;

using MahApps.Metro.Controls;

namespace LcLauncher.Main.Rack.Tile;

public class GroupEditViewModel: EditorViewModelBase
{
  public GroupEditViewModel(
    TileHostViewModel tileHost)
    : base(
        tileHost.Rack.Owner,
        "Group Tile - Editor",
        tileHost.Shelf.Theme)
  {
    TileHost = tileHost;
    if(tileHost.Tile is GroupTileViewModel groupTile)
    {
      ExistingGroup = groupTile;
      GroupId = groupTile.ChildTiles.Model.Id;
      Tooltip = groupTile.Tooltip ?? String.Empty;
      Title = groupTile.Title;
    }
    else
    {
      GroupId = Guid.NewGuid();
      Title = $"New Group {GroupId}";
      Tooltip = String.Empty;
    }
  }

  public TileHostViewModel TileHost { get; }

  public GroupTileViewModel? ExistingGroup { get; }

  public string Title {
    get => _title;
    set {
      if(SetValueProperty(ref _title, value))
      {
      }
    }
  }
  private string _title = string.Empty;

  public string Tooltip { // not null in this editor model
    get => _tooltip;
    set {
      if(SetValueProperty(ref _tooltip, value))
      {
      }
    }
  }
  private string _tooltip = string.Empty;

  public Guid GroupId {
    get => _groupId;
    set {
      if(SetValueProperty(ref _groupId, value))
      {
      }
    }
  }
  private Guid _groupId;

  public override bool CanAcceptEditor()
  {
    return
      !String.IsNullOrEmpty(Title)
      && (Title != ExistingGroup?.Title
        || Tooltip != (ExistingGroup?.Tooltip ?? String.Empty));
  }

  public override void AcceptEditor()
  {
    if(CanAcceptEditor())
    {
      if(ExistingGroup != null)
      {
        // update existing group
        ExistingGroup.Title = Title;
        ExistingGroup.Tooltip = String.IsNullOrEmpty(Tooltip) ? null : Tooltip;
        // force update
        TileHost.Tile = null;
        TileHost.Tile = ExistingGroup;
      }
      else
      {
        // create new group
        var tileData = new GroupData(
          GroupId,
          Title,
          String.IsNullOrEmpty(Tooltip) ? null : Tooltip);
        var newGroup = new GroupTileViewModel(
          TileHost.TileList,
          tileData);
        TileHost.Tile = newGroup;
        newGroup.IsActive = true;
      }
      TileHost.TileList.MarkDirty();
      IsActive = false;
    }
  }
}
