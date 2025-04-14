/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      { ShellLaunch: { } shellLaunch } =>
        LaunchTileViewModel.FromShell(ownerList, shellLaunch),
      { RawLaunch: { } rawLaunch } =>
        LaunchTileViewModel.FromRaw(ownerList, rawLaunch),
      { Group: { } group } =>
        new GroupTileViewModel(ownerList, group),
      { Quad: { } quadTile } => new QuadTileViewModel(ownerList, quadTile),
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
        OnHostChanged(oldHost, _host);
      }
    }
  }
  private TileHostViewModel? _host;

  /// <summary>
  /// Get a JSON-serializable model for this tile
  /// view model. This may be the original model the tile
  /// view model was created from, or a new model.
  /// In the case of an empty tile, this may be null.
  /// </summary>
  public abstract TileData? GetModel();

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

  protected virtual void OnHostChanged(
    TileHostViewModel? oldHost,
    TileHostViewModel? newHost)
  {
    // do nothing
  }
}
