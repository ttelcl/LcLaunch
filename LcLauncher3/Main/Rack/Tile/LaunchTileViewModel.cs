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
using System.Windows.Media.Imaging;

using LcLauncher.Launching;
using LcLauncher.WpfUtilities;

using LcLauncher.DataModel.Utilities;

using LcLauncher.DataModel.Entities;
using LcLauncher.IconTools;

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// Tile view model for launch tiles.
/// </summary>
public class LaunchTileViewModel: TileViewModel, IIconHost
{
  private LaunchTileViewModel(
    TileListViewModel ownerList,
    LaunchData model)
    : base(ownerList)
  {
    IconHostId = Guid.NewGuid();
    Model = model;
    Classification = LaunchKinds.GetLaunchKind(
      model.Target, !model.ShellMode);
    KindInfo = new LaunchKindInfo(Classification, model.Target);
    _title = Model.GetEffectiveTitle();
    _tooltip = Model.GetEffectiveTooltip();
    LoadIcon(IconCacheLoadLevel.FromCache);
    //EditCommandNew = new DelegateCommand(
    //  p => StartEditNew(),
    //  p => CanEditNew());
    FixIconCommand = new DelegateCommand(
      p => LoadIcon(IconCacheLoadLevel.LoadIfMissing),
      p => Host != null && !Host.Rack.HasMarkedItems);
    ForceIconCommand = new DelegateCommand(
      p => LoadIcon(IconCacheLoadLevel.LoadAlways),
      p => Host != null && !Host.Rack.HasMarkedItems);
    RunCommand = new DelegateCommand(
      p => RunTile(),
      p => Host != null && !Host.Rack.HasMarkedItems);
    ClickActionCommand = RunCommand;
  }

  public static LaunchTileViewModel FromLaunch(
    TileListViewModel ownerList,
    LaunchData model)
  {
    return new LaunchTileViewModel(ownerList, model);
  }

  //public ICommand EditCommandNew { get; }

  public ICommand FixIconCommand { get; }

  public ICommand ForceIconCommand { get; }

  public ICommand RunCommand { get; }

  /// <summary>
  /// The model for this tile.
  /// </summary>
  public LaunchData Model { get; }

  public LaunchKind Classification { get; }

  public LaunchKindInfo KindInfo { get; }

  public string Title {
    get => _title;
    set {
      if(SetValueProperty(ref _title, value))
      {
        // TODO: feed back to original and save
        Model.Title = value;
      }
    }
  }
  private string _title;

  public string Tooltip {
    get => _tooltip;
    set {
      if(SetValueProperty(ref _tooltip, value))
      {
        // TODO: feed back to original and save
        Model.Tooltip = value;
      }
    }
  }
  private string _tooltip;

  public override TileData? GetModel()
  {
    return Model switch {
      LaunchData launch => TileData.LaunchTile(launch),
      _ => throw new InvalidOperationException(
        $"Invalid launch data type {Model.GetType().FullName}")
    };
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

  public BitmapSource? IconSmall {
    get => _iconSmall;
    set {
      if(SetNullableInstanceProperty(ref _iconSmall, value))
      {
      }
    }
  }
  private BitmapSource? _iconSmall;

  public BitmapSource? IconMedium {
    get => _iconMedium;
    set {
      if(SetNullableInstanceProperty(ref _iconMedium, value))
      {
      }
    }
  }
  private BitmapSource? _iconMedium;

  /// <summary>
  /// Load the icon. The interpretation of the level is different than
  /// in the icon cache API: here the 'cache' includes the cache backing,
  /// and <see cref="IconCacheLoadLevel.LoadIfMissing"/> and 
  /// <see cref="IconCacheLoadLevel.LoadAlways"/> hit the OS icon load code if
  /// the icon is missing from the cache.
  /// </summary>
  /// <param name="level"></param>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  public void LoadIcon(IconCacheLoadLevel level)
  {
    var rack = OwnerList.Rack;
    var iconCache = rack.IconCache;

    // This now works very different from before. And should probably work in yet another way
    using var reader = iconCache.StartReader();

    switch(level)
    {
      case IconCacheLoadLevel.FromCache:
        {
          // load levels are interpreted softer in the cache, so pass a heavier code
          Icon = reader.FindIcon(Model.Icon48, IconCacheLoadLevel.LoadIfMissing);
          IconSmall = reader.FindIcon(Model.Icon16, IconCacheLoadLevel.LoadIfMissing);
          IconMedium = reader.FindIcon(Model.Icon32, IconCacheLoadLevel.LoadIfMissing);
          return;
        }
      case IconCacheLoadLevel.LoadIfMissing:
        {
          Icon = reader.FindIcon(Model.Icon48, IconCacheLoadLevel.LoadIfMissing);
          IconSmall = reader.FindIcon(Model.Icon16, IconCacheLoadLevel.LoadIfMissing);
          IconMedium = reader.FindIcon(Model.Icon32, IconCacheLoadLevel.LoadIfMissing);
          if(Icon != null && IconSmall != null && IconMedium != null)
          {
            return;
          }
          // TEMPORARILY DISABLED (return without doing anything)
          Trace.TraceWarning($"Not hard loading Icon! ({level})");
          //HardLoadIcon();
          //LoadIcon(IconLoadLevel.FromCache);
          return;
        }
      case IconCacheLoadLevel.LoadAlways:
        {
          // TEMPORARILY DISABLED (return without doing anything)
          Trace.TraceWarning($"Not hard loading Icon! ({level})");
          // PLACEHOLDERS FOR ICON EXTRACTION
          Icon = reader.FindIcon(Model.Icon48, IconCacheLoadLevel.LoadIfMissing);
          IconSmall = reader.FindIcon(Model.Icon16, IconCacheLoadLevel.LoadIfMissing);
          IconMedium = reader.FindIcon(Model.Icon32, IconCacheLoadLevel.LoadIfMissing);


          //HardLoadIcon();
          LoadIcon(IconCacheLoadLevel.FromCache);
          return;
        }
      default:
        throw new ArgumentOutOfRangeException(
          nameof(level), level, "Invalid icon load level");
    }
  }

  //private void HardLoadIcon()
  //{
  //  var iconCache = OwnerList.IconCache;
  //  var iconSource = Model.GetIconSource();
  //  var hashes = iconCache.CacheIcons(iconSource, IconSize.Normal);
  //  if(hashes == null)
  //  {
  //    // Clear all icons - they are no longer valid
  //    if(!String.IsNullOrEmpty(Model.Icon16)
  //      || !String.IsNullOrEmpty(Model.Icon32)
  //      || !String.IsNullOrEmpty(Model.Icon48))
  //    {
  //      OwnerList.MarkDirty();
  //    }
  //    Model.Icon48 = null;
  //    Model.Icon32 = null;
  //    Model.Icon16 = null;
  //    Trace.TraceError(
  //      $"Failed to load icon for {iconSource}");
  //    Icon = null;
  //    return;
  //  }
  //  if(Model.Icon48 != hashes.Large)
  //  {
  //    Model.Icon48 = hashes.Large;
  //    OwnerList.MarkDirty();
  //  }
  //  if(Model.Icon32 != hashes.Medium)
  //  {
  //    Model.Icon32 = hashes.Medium;
  //    OwnerList.MarkDirty();
  //  }
  //  if(Model.Icon16 != hashes.Small)
  //  {
  //    Model.Icon16 = hashes.Small;
  //    OwnerList.MarkDirty();
  //  }
  //}

  public string FallbackIcon => Model switch {
    LaunchData ld => ld.ShellMode ? "RocketLaunch" : "RocketLaunchOutline",
    _ => "Help"
  };

  public override string PlainIcon { get => FallbackIcon; }

  ///// <summary>
  ///// This implementation returns zero or one icon load job
  ///// </summary>
  //public override IEnumerable<IconLoadJob> GetIconLoadJobs(
  //  bool reload)
  //{
  //  if(reload)
  //  {
  //    yield return new IconLoadJob(
  //      OwnerList,
  //      this,
  //      () => { LoadIcon(IconLoadLevel.LoadAlways); });
  //  }
  //  else
  //  {
  //    var hasIcon = Icon != null;
  //    if(!hasIcon)
  //    {
  //      yield return new IconLoadJob(
  //        OwnerList,
  //        this,
  //        () => { LoadIcon(IconLoadLevel.LoadIfMissing); });
  //    }
  //  }
  //}

  public Guid IconHostId { get; }

  private bool CanEditNew()
  {
    if(Host == null)
    {
      return false;
    }
    if(GetIsKeyTile())
    {
      return false;
    }
    return Classification switch {
      LaunchKind.Invalid => false,
      LaunchKind.Document => true,
      LaunchKind.ShellApplication => true,
      LaunchKind.Raw => true,
      LaunchKind.UriKind => true,
      _ => false,
    };
  }

  //private void StartEditNew()
  //{
  //  if(!CanEditNew())
  //  {
  //    return;
  //  }
  //  EditorViewModelBase editor;
  //  editor = new LaunchEditViewModel(Host!);
  //  editor.IsActive = true;
  //}

  private void RunTile()
  {
    if(Model is LaunchData launch)
    {
      Launcher.Launch(launch);
    }
    else
    {
      throw new InvalidOperationException(
        "Unrecognized launch tile data type");
    }
  }
}
