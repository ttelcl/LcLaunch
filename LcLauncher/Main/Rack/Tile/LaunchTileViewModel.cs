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
/// Shared tile view model for both kinds of old launch tiles, as well
/// as for the new launch tile model.
/// </summary>
public class LaunchTileViewModel: TileViewModel, IIconHost
{
  private LaunchTileViewModel(
    TileListViewModel ownerList,
    ILaunchData model)
    : base(ownerList)
  {
    IconHostId = Guid.NewGuid();
    Model = model;
    if(model is ShellLaunch shell)
    {
      ShellModel = shell;
      RawModel = null;
      NewModel = shell.ToLaunch();
      Classification = LaunchData.GetLaunchKind(
        shell.TargetPath, false);
      KindInfo = new LaunchKindInfo(Classification, shell.TargetPath);
    }
    else if(model is RawLaunch raw)
    {
      ShellModel = null;
      RawModel = raw;
      NewModel = raw.ToLaunch();
      Classification = LaunchData.GetLaunchKind(
        raw.TargetPath, true);
      KindInfo = new LaunchKindInfo(Classification, raw.TargetPath);
    }
    else if(model is LaunchData launch)
    {
      ShellModel = launch.ToShellLaunch();
      RawModel = launch.ToRawLaunch();
      NewModel = launch;
      Classification = LaunchData.GetLaunchKind(
        launch.Target, !launch.ShellMode);
      KindInfo = new LaunchKindInfo(Classification, launch.Target);
    }
    else
    {
      ShellModel = null;
      RawModel = null;
      NewModel = null;
      Classification = LaunchKind.Invalid;
      KindInfo = new LaunchKindInfo(LaunchKind.Invalid, "");
    }
    _title = Model.GetEffectiveTitle();
    _tooltip = Model.GetEffectiveTooltip();
    LoadIcon(IconLoadLevel.FromCache);
    EditCommand = new DelegateCommand(
      p => StartEdit(),
      p => CanEdit());
    EditCommandNew = new DelegateCommand(
      p => StartEditNew(),
      p => CanEditNew());
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

  public ICommand EditCommandNew { get; }

  public ICommand FixIconCommand { get; }

  public ICommand ForceIconCommand { get; }

  public ICommand RunCommand { get; }

  /// <summary>
  /// The model for this tile.
  /// </summary>
  public ILaunchData Model { get; }

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
      ShellLaunch shell => TileData.ShellTile(shell),
      RawLaunch raw => TileData.RawTile(raw),
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

  public void LoadIcon(IconLoadLevel level)
  {
    //var hasIcon = Icon != null;
    var hasHash = Model.Icon48 != null;
    var iconCache = OwnerList.IconCache;
    switch(level)
    {
      case IconLoadLevel.FromCache:
        {
          if(/*hasIcon ||*/ !hasHash)
          {
            return;
          }
          var icon = iconCache.LoadCachedIcon(Model.Icon48);
          Icon = icon;
          IconSmall = iconCache.LoadCachedIcon(Model.Icon16);
          IconMedium = iconCache.LoadCachedIcon(Model.Icon32);
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
            var icon = iconCache.LoadCachedIcon(Model.Icon48);
            if(icon != null)
            {
              Icon = icon;
              IconSmall = iconCache.LoadCachedIcon(Model.Icon16);
              IconMedium = iconCache.LoadCachedIcon(Model.Icon32);
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
    var iconSource = Model.GetIconSource();
    var hashes = iconCache.CacheIcons(iconSource, IconSize.Normal);
    if(hashes == null)
    {
      // Clear all icons - they are no longer valid
      if(!String.IsNullOrEmpty(Model.Icon16)
        || !String.IsNullOrEmpty(Model.Icon32)
        || !String.IsNullOrEmpty(Model.Icon48))
      {
        OwnerList.MarkDirty();
      }
      Model.Icon48 = null;
      Model.Icon32 = null;
      Model.Icon16 = null;
      Trace.TraceError(
        $"Failed to load icon for {iconSource}");
      Icon = null;
      return;
    }
    if(Model.Icon48 != hashes.Large)
    {
      Model.Icon48 = hashes.Large;
      OwnerList.MarkDirty();
    }
    if(Model.Icon32 != hashes.Medium)
    {
      Model.Icon32 = hashes.Medium;
      OwnerList.MarkDirty();
    }
    if(Model.Icon16 != hashes.Small)
    {
      Model.Icon16 = hashes.Small;
      OwnerList.MarkDirty();
    }
  }

  public string FallbackIcon => Model switch {
    ShellLaunch => "RocketLaunch",
    RawLaunch => "RocketLaunchOutline",
    LaunchData => "RocketLaunchOutline",
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

  private void StartEditNew()
  {
    if(!CanEditNew())
    {
      return;
    }
    EditorViewModelBase editor;
    editor = new LaunchEditViewModel(Host!);
    editor.IsActive = true;
  }

  private void RunTile()
  {
    if(Model is ShellLaunch shell)
    {
      Launcher.Launch(shell);
    }
    else if(Model is RawLaunch raw)
    {
      Launcher.Launch(raw);
    }
    else if(Model is LaunchData launch)
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
