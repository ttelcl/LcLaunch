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
/// A child VM of <see cref="LaunchTileViewModel"/> that can be
/// embedded in a <see cref="GroupIconViewModel"/> to show
/// the small icon of the launch tile in the group tile.
/// </summary>
public class GroupTileAdapterViewModel: ViewModelBase
{
  /// <summary>
  /// Create a new GroupTileAdapterViewModel
  /// </summary>
  public GroupTileAdapterViewModel(
    BitmapSource? icon)
  {
  }

  public BitmapSource? Icon {
    get => _icon;
    set {
      if(SetNullableInstanceProperty(ref _icon, value))
      {
      }
    }
  }
  private BitmapSource? _icon;

}
