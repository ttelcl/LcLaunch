/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// A mini icon to be shown in a group tile.
/// </summary>
public class GroupIconViewModel: ViewModelBase
{
  public GroupIconViewModel(
    GroupTileViewModel owner,
    TileViewModel? target)
  {
    Owner = owner;
    Target = target;
  }

  public GroupTileViewModel Owner { get; }

  public TileViewModel? Target { get; }

  public string PlainIcon {
    get => Target switch {
      null => "EggOutline",
      EmptyTileViewModel evm => evm.Icon,
      LaunchTileViewModel lvm => lvm.FallbackIcon,
      GroupTileViewModel _ => "DotsGrid",
      QuadTileViewModel _ => "ViewGrid",
      _ => "Help"
    };
  }

  public BitmapSource? Icon {
    get => Target switch {
      null => null,
      EmptyTileViewModel _ => null,
      LaunchTileViewModel lvm => lvm.IconSmall,
      GroupTileViewModel _ => null,
      QuadTileViewModel _ => null,
      _ => null
    };
  }

}
