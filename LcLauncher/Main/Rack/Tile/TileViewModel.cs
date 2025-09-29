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

using LcLauncher.WpfUtilities;

using LcLauncher.DataModel.Entities;
using LcLauncher.DataModel.ChangeTracking;

namespace LcLauncher.Main.Rack.Tile;

public abstract class TileViewModel: ViewModelBase, IDirtyPart
{
  protected TileViewModel(
    TileListViewModel ownerList)
  {
    OwnerList=ownerList;
  }

  public static TileViewModel Create(
    TileListViewModel ownerList,
    TileData? model)
  {
    return model switch {
      null => new EmptyTileViewModel(ownerList, null),
      { Launch: { } launch } =>
        LaunchTileViewModel.FromLaunch(ownerList, launch),
      { Group: { } group } =>
        new GroupTileViewModel(ownerList, group),
      { Quad: { } quadTile } =>
        new QuadTileViewModel(ownerList, quadTile),

      _ => new EmptyTileViewModel(ownerList, model)
    };
  }

  public TileListViewModel OwnerList { get; }

  public RackViewModel Rack => OwnerList.Rack;

  public TileHostViewModel? Host {
    get => _host;
    internal set {
      var oldHost = _host;
      if(SetNullableInstanceProperty(ref _host, value))
      {
        HostHovering = Host != null && Host.Hovering;
        OnHostChanged(oldHost, _host);
      }
    }
  }
  private TileHostViewModel? _host;

  /// <summary>
  /// The plain icon to use if no custom icon is available.
  /// </summary>
  public abstract string PlainIcon { get; }

  /// <summary>
  /// Get a JSON-serializable model for this tile
  /// view model. This may be the original model the tile
  /// view model was created from, or a new model.
  /// In the case of an empty tile, this may be null.
  /// ((named 'GetModel()' for historical reasons. GetEntity() would be better))
  /// </summary>
  public abstract TileData? GetModel();

  public bool GetIsKeyTile()
  {
    return Host != null && Host.IsKeyTile;
  }

  public virtual void OnHoveringChanged(bool hovering)
  {
    // do nothing
  }

  /// <summary>
  /// True if the tile can be selected.
  /// True if there is a Host, but subclasses can be more restrictive.
  /// </summary>
  /// <returns></returns>
  public virtual bool CanSelectTile()
  {
    return Host != null;
  }

  private bool _hostHovering;
  public bool HostHovering {
    get => _hostHovering;
    set {
      if(SetValueProperty(ref _hostHovering, value))
      {
        if(!_hostHovering)
        {
          // reset priming if mouse leaves the host tile before going up again
          IsPrimed = false;
        }
        OnHoveringChanged(_hostHovering);
      }
    }
  }

  /// <summary>
  /// The click action for this tile (one of the other
  /// commands). Or null to not act like a button.
  /// </summary>
  public ICommand? ClickActionCommand { get; protected set; } = null;

  ///// <summary>
  ///// Called by host tile when the mouse button state changes and
  ///// <see cref="ClickActionCommand"/> is not null.
  ///// </summary>
  ///// <param name="down">
  ///// True at mouse-down, false at mouse-up.
  ///// </param>
  //public void MouseButtonChange(bool down, ModifierKeys modifierKeys)
  //{
  //  //Trace.TraceInformation(
  //  //  $"TileViewModel: MouseButtonChange {down}");
  //  var trigger =
  //    !down
  //    && IsPrimed
  //    && HostHovering
  //    && Host!=null
  //    && ClickActionCommand!=null;
  //  IsPrimed = down && HostHovering && ClickActionCommand!=null;
  //  if(trigger && ClickActionCommand!.CanExecute(null))
  //  {
  //    ClickActionCommand!.Execute(null);
  //  }
  //}

  /// <summary>
  /// A click on this tile is between mousedown and mouseup.
  /// Managed through the tile host.
  /// Used by the tile host and by this tile view itself.
  /// </summary>
  public bool IsPrimed {
    get => _isPrimed;
    set {
      if(SetValueProperty(ref _isPrimed, value))
      {
        //Trace.TraceInformation(
        //  $"TileViewModel: IsPrimed changed to {value}");
      }
    }
  }
  private bool _isPrimed;

  /// <summary>
  /// The tile host has detected a click on this tile
  /// </summary>
  /// <param name="modifierKeys"></param>
  public void TileClicked(ModifierKeys modifierKeys)
  {
    if(Host != null)
    {
      if(Host.IsKeyTile)
      {
        // Click on key tile. Ignore tile specifics and deselect the tile
        // instead.
        Host.IsKeyTile = false;
      }
      else if(modifierKeys.HasFlag(ModifierKeys.Control))
      {
        // Control-click on tile that is not the key tile: make it the key tile
        // if allowed.
        if(CanSelectTile())
        {
          Host.IsKeyTile = true;
        }
        else
        {
          Trace.TraceWarning(
            "Ignoring CTRL-Click because tile is denying being selected");
        }
      }
      else
      {
        ClickActionCommand?.Execute(null);
      }
    }
    else
    {
      // Should never happen
      Trace.TraceError(
        "Ignoring click on disconected tile");
    }
  }

  protected virtual void OnHostChanged(
    TileHostViewModel? oldHost,
    TileHostViewModel? newHost)
  {
    // do nothing
  }

  public IDirtyHost? DirtyHost => Host?.DirtyHost;

  public void MarkAsDirty()
  {
    DirtyHost?.MarkAsDirty();
  }
}
