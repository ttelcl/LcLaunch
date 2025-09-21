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
using LcLauncher.Persistence;
using LcLauncher.WpfUtilities;

using LcLauncher.DataModel.Utilities;

using Model2 = LcLauncher.ModelsV2;
using Model3 = LcLauncher.DataModel.Entities;

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// Shared tile view model for both kinds of old launch tiles, as well
/// as for the new launch tile model.
/// </summary>
public class LaunchTileViewModel: TileViewModel, IIconHost
{
  private LaunchTileViewModel(
    TileListViewModel ownerList,
    Model2.LaunchData model)
    : base(ownerList)
  {
    IconHostId = Guid.NewGuid();
    Model = model;
    Classification = LaunchKinds.GetLaunchKind(
      model.Target, !model.ShellMode);
    KindInfo = new LaunchKindInfo(Classification, model.Target);
    _title = Model.GetEffectiveTitle();
    _tooltip = Model.GetEffectiveTooltip();
    LoadIcon(IconLoadLevel.FromCache);
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

  public static LaunchTileViewModel FromLaunch(
    TileListViewModel ownerList,
    Model2.LaunchData model)
  {
    return new LaunchTileViewModel(ownerList, model);
  }

  public ICommand EditCommandNew { get; }

  public ICommand FixIconCommand { get; }

  public ICommand ForceIconCommand { get; }

  public ICommand RunCommand { get; }

  /// <summary>
  /// The model for this tile.
  /// </summary>
  public Model2.LaunchData Model { get; }

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

  public override Model2.TileData? GetModel()
  {
    return Model switch {
      Model2.LaunchData launch => Model2.TileData.LaunchTile(launch),
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
    Model2.LaunchData ld => ld.ShellMode ? "RocketLaunch" : "RocketLaunchOutline",
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
    if(Model is Model2.LaunchData launch)
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
