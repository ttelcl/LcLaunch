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
using LcLauncher.Main.Rack.Editors;

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// Tile view model for launch tiles.
/// </summary>
public class LaunchTileViewModel:
  TileViewModel, IIconJobTarget, ICanQueueIcons
{
  private LaunchTileViewModel(
    TileListViewModel ownerList,
    LaunchData model)
    : base(ownerList)
  {
    IconTargetId = Guid.NewGuid();
    Model = model;
    IconIds = new IconIdSet {
      Icon16 = model.Icon16,
      Icon32 = model.Icon32,
      Icon48 = model.Icon48,
      Icon256 = null,
    };
    Icons = new IconSet {
      IconSmall = IconSmall,
      IconMedium = IconMedium,
      IconLarge = Icon,
      IconExtraLarge = null,
    };
    Classification = LaunchKinds.GetLaunchKind(
      model.Target, !model.ShellMode);
    KindInfo = new LaunchKindInfo(Classification, model.Target);
    _title = Model.GetEffectiveTitle();
    _tooltip = Model.GetEffectiveTooltip();
    GroupTileAdapter = new GroupTileAdapterViewModel(IconSmall);

    EditCommandNew = new DelegateCommand(
      p => StartEditNew(),
      p => CanEditNew());
    FixIconCommand = new DelegateCommand(
      p => QueueIcon(IconLoadLevel.System, false),
      p => Host != null && !Host.Rack.HasMarkedItems);
    ForceIconCommand = new DelegateCommand(
      p => QueueIcon(IconLoadLevel.System, true),
      p => Host != null && !Host.Rack.HasMarkedItems);
    RunCommand = new DelegateCommand(
      p => RunTile(),
      p => Host != null && !Host.Rack.HasMarkedItems);
    ClickActionCommand = RunCommand;

    QueueIcons(false);
  }

  public static LaunchTileViewModel FromLaunch(
    TileListViewModel ownerList,
    LaunchData model)
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
        Icons.IconLarge = value;
        RaisePropertyChanged(nameof(Icons));
      }
    }
  }
  private BitmapSource? _icon;

  public BitmapSource? IconSmall {
    get => _iconSmall;
    set {
      if(SetNullableInstanceProperty(ref _iconSmall, value))
      {
        Icons.IconSmall = value;
        RaisePropertyChanged(nameof(Icons));
        GroupTileAdapter.Icon = value;
      }
    }
  }
  private BitmapSource? _iconSmall;

  public BitmapSource? IconMedium {
    get => _iconMedium;
    set {
      if(SetNullableInstanceProperty(ref _iconMedium, value))
      {
        Icons.IconMedium = value;
        RaisePropertyChanged(nameof(Icons));
      }
    }
  }
  private BitmapSource? _iconMedium;

  public GroupTileAdapterViewModel GroupTileAdapter { get; }

  /// <inheritdoc/>
  public void QueueIcons(bool regenerate)
  {
    if(regenerate)
    {
      QueueIcon(IconLoadLevel.System, true);
    }
    else
    {
      QueueIcon(IconLoadLevel.System, false);
    }
  }

  public void QueueIcon(IconLoadLevel level, bool refresh)
  {
    var rack = OwnerList.Rack;
    var queue = rack.IconQueue;
    queue.Enqueue(
      this,
      level,
      refresh);
    rack.Owner.ActivateRackIconQueue();
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

  /// <inheritdoc/>
  public Guid IconTargetId { get; }

  /// <inheritdoc/>
  public string IconSource => Model.GetIconSource();

  /// <inheritdoc/>
  public IconSize IconSizes => IconSize.Normal;

  /// <inheritdoc/>
  public IconIdSet IconIds { get; }

  /// <inheritdoc/>
  public IconSet Icons { get; }

  public void UpdateIcons(IconIdSet iconIds, IconSet icons)
  {
    if(iconIds.Icon16 != IconIds.Icon16)
    {
      IconIds.Icon16 = iconIds.Icon16;
      MarkAsDirty();
    }
    if(iconIds.Icon32 != IconIds.Icon32)
    {
      IconIds.Icon32 = iconIds.Icon32;
      MarkAsDirty();
    }
    if(iconIds.Icon48 != IconIds.Icon48)
    {
      IconIds.Icon48 = iconIds.Icon48;
      MarkAsDirty();
    }
    if(icons.IconSmall != IconSmall)
    {
      IconSmall = icons.IconSmall;
    }
    if(icons.IconMedium != IconMedium)
    {
      IconMedium = icons.IconMedium;
    }
    if(icons.IconLarge != Icon)
    {
      Icon = icons.IconLarge;
    }

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
