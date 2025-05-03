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

using LcLauncher.IconUpdates;
using LcLauncher.Launching;
using LcLauncher.Models;
using LcLauncher.Persistence;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// Shared tile view model for both launch tiles.
/// </summary>
public class LaunchTileViewModel: TileViewModel, IIconHost
{
  private LaunchTileViewModel(
    TileListViewModel ownerList,
    ILaunchData model)
    : base(ownerList)
  {
    IconHostId = Guid.NewGuid();
    OldModel = model;
    if(model is ShellLaunch shell)
    {
      ShellModel = shell;
      RawModel = null;
      NewModel = shell.ToLaunch();
      Classification = LaunchData.GetLaunchKind(
        model.TargetPath, false);
    }
    else if(model is RawLaunch raw)
    {
      ShellModel = null;
      RawModel = raw;
      NewModel = raw.ToLaunch();
      Classification = LaunchData.GetLaunchKind(
        model.TargetPath, true);
    }
    else if(model is LaunchData launch)
    {
      ShellModel = launch.ToShellLaunch();
      RawModel = launch.ToRawLaunch();
      NewModel = launch;
      Classification = LaunchData.GetLaunchKind(
        model.TargetPath, !launch.ShellMode);
    }
    else
    {
      ShellModel = null;
      RawModel = null;
      Classification = LaunchKind.Invalid;
    }
    _title = OldModel.GetEffectiveTitle();
    _tooltip = OldModel.GetEffectiveTooltip();
    LoadIcon(IconLoadLevel.FromCache);
    EditCommand = new DelegateCommand(
      p => StartEdit(),
      p => CanEdit());
    FixIconCommand = new DelegateCommand(
      p => LoadIcon(IconLoadLevel.LoadIfMissing),
      p => Host != null && !Host.Rack.HasMarkedItems);
    ForceIconCommand = new DelegateCommand(
      p => LoadIcon(IconLoadLevel.LoadAlways),
      p => Host != null && !Host.Rack.HasMarkedItems);
    RunCommand = new DelegateCommand(
      p => RunTile(),
      p => Host != null && !Host.Rack.HasMarkedItems);
    ClickActionCommand = RunCommand;
  }

  public static LaunchTileViewModel FromShell(
    TileListViewModel ownerList,
    ShellLaunch model)
  {
    Trace.TraceWarning(
      $"OBSOLETE! Creating launch tile from shell launch");
    return new LaunchTileViewModel(ownerList, model);
  }

  public static LaunchTileViewModel FromRaw(
    TileListViewModel ownerList,
    RawLaunch model)
  {
    Trace.TraceWarning(
      $"OBSOLETE! Creating launch tile from raw launch");
    return new LaunchTileViewModel(ownerList, model);
  }

  public static LaunchTileViewModel FromLaunch(
    TileListViewModel ownerList,
    LaunchData model)
  {
    return new LaunchTileViewModel(ownerList, model);
  }

  public ICommand EditCommand { get; }

  public ICommand FixIconCommand { get; }

  public ICommand ForceIconCommand { get; }

  public ICommand RunCommand { get; }

  /// <summary>
  /// The model for this tile.
  /// </summary>
  public ILaunchData OldModel { get; }

  /// <summary>
  /// The model for this tile, if it is a shell launch.
  /// </summary>
  public IShellLaunchData? ShellModel { get; }

  /// <summary>
  /// The model for this tile, if it is a raw launch.
  /// </summary>
  public IRawLaunchData? RawModel { get; }

  public LaunchData? NewModel { get; }

  public LaunchKind Classification { get; }

  public string Title {
    get => _title;
    set {
      if(SetValueProperty(ref _title, value))
      {
        // TODO: feed back to original and save
        OldModel.Title = value;
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
        OldModel.Tooltip = value;
      }
    }
  }
  private string _tooltip;

  public override TileData? GetModel()
  {
    return OldModel switch {
      LaunchData launch => TileData.LaunchTile(launch),
      ShellLaunch shell => TileData.ShellTile(shell),
      RawLaunch raw => TileData.RawTile(raw),
      _ => throw new InvalidOperationException(
        $"Invalid launch data type {OldModel.GetType().FullName}")
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

  public void LoadIcon(IconLoadLevel level)
  {
    //var hasIcon = Icon != null;
    var hasHash = OldModel.Icon48 != null;
    var iconCache = OwnerList.IconCache;
    switch(level)
    {
      case IconLoadLevel.FromCache:
        {
          if(/*hasIcon ||*/ !hasHash)
          {
            return;
          }
          var icon = iconCache.LoadCachedIcon(OldModel.Icon48);
          Icon = icon;
          IconSmall = iconCache.LoadCachedIcon(OldModel.Icon16);
          IconMedium = iconCache.LoadCachedIcon(OldModel.Icon32);
          return;
        }
      case IconLoadLevel.LoadIfMissing:
        {
          //if(hasIcon)
          //{
          //  return;
          //}
          if(hasHash)
          {
            var icon = iconCache.LoadCachedIcon(OldModel.Icon48);
            if(icon != null)
            {
              Icon = icon;
              IconSmall = iconCache.LoadCachedIcon(OldModel.Icon16);
              IconMedium = iconCache.LoadCachedIcon(OldModel.Icon32);
              return;
            }
          }
          HardLoadIcon();
          LoadIcon(IconLoadLevel.FromCache);
          return;
        }
      case IconLoadLevel.LoadAlways:
        {
          HardLoadIcon();
          LoadIcon(IconLoadLevel.FromCache);
          return;
        }
      default:
        throw new ArgumentOutOfRangeException(
          nameof(level), level, "Invalid icon load level");
    }
  }

  private void HardLoadIcon()
  {
    var iconCache = OwnerList.IconCache;
    var iconSource = OldModel.GetIconSource();
    var hashes = iconCache.CacheIcons(iconSource, IconSize.Normal);
    if(hashes == null)
    {
      // Clear all icons - they are no longer valid
      if(!String.IsNullOrEmpty(OldModel.Icon16)
        || !String.IsNullOrEmpty(OldModel.Icon32)
        || !String.IsNullOrEmpty(OldModel.Icon48))
      {
        OwnerList.MarkDirty();
      }
      OldModel.Icon48 = null;
      OldModel.Icon32 = null;
      OldModel.Icon16 = null;
      Trace.TraceError(
        $"Failed to load icon for {iconSource}");
      Icon = null;
      return;
    }
    if(OldModel.Icon48 != hashes.Large)
    {
      OldModel.Icon48 = hashes.Large;
      OwnerList.MarkDirty();
    }
    if(OldModel.Icon32 != hashes.Medium)
    {
      OldModel.Icon32 = hashes.Medium;
      OwnerList.MarkDirty();
    }
    if(OldModel.Icon16 != hashes.Small)
    {
      OldModel.Icon16 = hashes.Small;
      OwnerList.MarkDirty();
    }
  }

  public string FallbackIcon => OldModel switch {
    ShellLaunch => "RocketLaunch",
    RawLaunch => "RocketLaunchOutline",
    _ => "Help"
  };

  public override string PlainIcon { get => FallbackIcon; }

  /// <summary>
  /// This implementation returns zero or one icon load job
  /// </summary>
  public override IEnumerable<IconLoadJob> GetIconLoadJobs(
    bool reload)
  {
    if(reload)
    {
      yield return new IconLoadJob(
        OwnerList,
        this,
        () => { LoadIcon(IconLoadLevel.LoadAlways); });
    }
    else
    {
      var hasIcon = Icon != null;
      if(!hasIcon)
      {
        yield return new IconLoadJob(
          OwnerList,
          this,
          () => { LoadIcon(IconLoadLevel.LoadIfMissing); });
      }
    }
  }

  public Guid IconHostId { get; }

  private bool CanEdit()
  {
    if(Host == null)
    {
      return false;
    }
    if(GetIsKeyTile())
    {
      return false;
    }
    if(ShellModel != null)
    {
      return Classification switch {
        LaunchKind.Document => true,
        LaunchKind.ShellApplication => false, // NYI
        LaunchKind.Raw => false, // should never happen
        _ => false,
      };
    }
    else if(RawModel != null)
    {
      return Classification switch {
        LaunchKind.Document => false, // should never happen
        LaunchKind.ShellApplication => false, // should never happen
        LaunchKind.Raw => true,
        _ => false,
      };
    }
    else
    {
      return false;
    }
  }

  private void StartEdit()
  {
    if(!CanEdit())
    {
      return;
    }
    EditorViewModelBase editor;
    if(ShellModel != null)
    {
      editor = new LaunchDocumentViewModel(Host!);
    }
    else if(RawModel != null)
    {
      editor = new LaunchExeViewModel(Host!);
    }
    else
    {
      throw new InvalidOperationException(
        "Unrecognized launch tile data type");
    }
    editor.IsActive = true;
  }

  private void RunTile()
  {
    if(OldModel is ShellLaunch shell)
    {
      Launcher.Launch(shell);
    }
    else if(OldModel is RawLaunch raw)
    {
      Launcher.Launch(raw);
    }
    else if(OldModel is LaunchData launch)
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
