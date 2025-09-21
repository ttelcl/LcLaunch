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

using LcLauncher.IconUpdates;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

public abstract class TileViewModel: ViewModelBase, IIconLoadJobSource
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
  /// </summary>
  public abstract TileData? GetModel();

  public bool GetIsKeyTile()
  {
    return Host != null && Host.IsKeyTile;
  }

  /// <summary>
  /// Create Icon Load jobs for this tile. The default
  /// implementation returns an empty list. Icon load jobs
  /// create missing icon(s), store them in the cache, and
  /// update the icon hashes in the tile data.
  /// </summary>
  /// <param name="reload">
  /// If true, reload the icon into the cache even if it
  /// already is known.
  /// </param>
  /// <returns></returns>
  public virtual IEnumerable<IconLoadJob> GetIconLoadJobs(
    bool reload)
  {
    return [];
  }

  public IconLoadQueue IconLoadQueue { get => OwnerList.IconLoadQueue; }

  public virtual void OnHoveringChanged(bool hovering)
  {
    // do nothing
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

  /// <summary>
  /// Called by host tile when the mouse button state changes and
  /// <see cref="ClickActionCommand"/> is not null.
  /// </summary>
  /// <param name="down">
  /// True at mouse-down, false at mouse-up.
  /// </param>
  public void MouseButtonChange(bool down)
  {
    //Trace.TraceInformation(
    //  $"TileViewModel: MouseButtonChange {down}");
    var trigger =
      !down
      && IsPrimed
      && HostHovering
      && Host!=null
      && ClickActionCommand!=null;
    IsPrimed = down && HostHovering && ClickActionCommand!=null;
    if(trigger && ClickActionCommand!.CanExecute(null))
    {
      ClickActionCommand!.Execute(null);
    }
  }

  private bool _isPrimed;
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

  protected virtual void OnHostChanged(
    TileHostViewModel? oldHost,
    TileHostViewModel? newHost)
  {
    // do nothing
  }
}
