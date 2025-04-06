/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;

using Newtonsoft.Json;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main;

// Temporary helper class

public class TestPaneViewModel: ViewModelBase
{
  public TestPaneViewModel(
    MainViewModel host)
  {
    Host = host;
    ResetShelf1Command = new DelegateCommand(p => ResetShelf1());
    LoadShelfFileCommand = new DelegateCommand(p => LoadShelfFile());
    OpenIconFileCommand = new DelegateCommand(p => OpenIconFile());
    TestTestTilesCommand = new DelegateCommand(p => ScanTestTiles());
    LoadDemoRackCommand = new DelegateCommand(p => LoadDemoRack());
  }

  public MainViewModel Host { get; }

  public static Guid TileTestGuid { get; } =
    new Guid("61bd78c5-43e7-4627-bbd3-74e772abb123");

  public string? IconFile {
    get => _iconFile;
    set {
      if(SetValueProperty(ref _iconFile, value))
      {
        ProbeIconFile();
      }
    }
  }
  private string? _iconFile;

  private void ProbeIconFile()
  {
    if(File.Exists(IconFile))
    {
      try
      {
        using(var iconShell = ShellObject.FromParsingName(IconFile))
        {
          Trace.TraceInformation(
            $"Parsing Name: {iconShell.ParsingName}");
          Trace.TraceInformation(
            $"Display Name: {iconShell.Name}");
          var thumbnail = iconShell.Thumbnail;
          thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
          IconSmall = thumbnail.SmallBitmapSource;
          Trace.TraceInformation(
            $"Small Icon: {IconSmall.Width}"); // 16
          IconMedium = thumbnail.MediumBitmapSource;
          Trace.TraceInformation(
            $"Medium Icon: {IconMedium.Width}"); // 32
          IconLarge = thumbnail.LargeBitmapSource;
          Trace.TraceInformation(
            $"Large Icon: {IconLarge.Width}"); // 48
          IconExtraLarge = thumbnail.ExtraLargeBitmapSource;
          Trace.TraceInformation(
            $"Extra Large Icon: {IconExtraLarge.Width}"); // 256
        }
      }
      catch(Exception ex)
      {
        Trace.TraceError(
          $"ProbeIconFile: Error probing icon file: {ex}");
        ClearIcons();
      }
    }
    else
    {
      ClearIcons();
    }
  }

  private void ClearIcons()
  {
    IconSmall = null;
    IconMedium = null;
    IconLarge = null;
    IconExtraLarge = null;
  }

  /// <summary>
  /// Smallest size, 16x16 pixels usually.
  /// Too small for most purposes.
  /// </summary>
  public BitmapSource? IconSmall {
    get => _iconSmall;
    set {
      if(SetNullableInstanceProperty(ref _iconSmall, value))
      {
      }
    }
  }
  private BitmapSource? _iconSmall;

  /// <summary>
  /// Smaller size but still readable, 32x32 pixels usually.
  /// May be of use for group thumbnails.
  /// </summary>
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
  /// Most relevant size, 48x48 pixels usually.
  /// </summary>
  public BitmapSource? IconLarge {
    get => _iconLarge;
    set {
      if(SetNullableInstanceProperty(ref _iconLarge, value))
      {
      }
    }
  }
  private BitmapSource? _iconLarge;

  /// <summary>
  /// Huge icon, 256x256 pixels usually. May be of use as source
  /// for scaling down to other sizes.
  /// </summary>
  public BitmapSource? IconExtraLarge {
    get => _iconExtraLarge;
    set {
      if(SetNullableInstanceProperty(ref _iconExtraLarge, value))
      {
      }
    }
  }
  private BitmapSource? _iconExtraLarge;

  public ICommand ResetShelf1Command { get; }

  public ICommand LoadShelfFileCommand { get; }

  public ICommand OpenIconFileCommand { get; }

  public ICommand TestTestTilesCommand { get; }

  public ICommand LoadDemoRackCommand { get; }

  private void ResetShelf1()
  {
    Host.ColumnA.DbgShelfA = new ShelfViewModel(
      Host.ColumnA,
      new ShelfData0("Test Shelf", [], theme: "Olive"));
  }

  private void LoadShelfFile()
  {
    OpenFileDialog ofd = new OpenFileDialog() {
      Filter = "Shelf files (*.shelf.json)|*.shelf.json",
      Title = "Open Shelf File",
      CheckFileExists = true,
      CheckPathExists = true,
      Multiselect = false,
    };
    var result = ofd.ShowDialog();
    if(result == true)
    {
      var data = ShelfData0.LoadFile(ofd.FileName);
      var reserialized = JsonConvert.SerializeObject(data, Formatting.Indented);
      Trace.TraceInformation(
        $"Shelf content: {reserialized}");

      // using data is NYI - placeholder
      var shelf = Host.ColumnA.DbgShelfA;
      shelf.ShelfData = data;

    }
  }

  private void OpenIconFile()
  {
    var ofd = new OpenFileDialog() {
      Filter = "Any file (*.*)|*.*",
      Title = "Open Icon File",
      CheckFileExists = true,
      CheckPathExists = true,
      Multiselect = false,
      DereferenceLinks = false,
    };
    var result = ofd.ShowDialog();
    if(result == true)
    {
      IconFile = ofd.FileName;
    }
  }

  private TileListModel? LoadTestTiles()
  {
    var tiles = TileListModel.Load(Host.Store, TileTestGuid);
    return tiles;
  }

  private void ScanTestTiles()
  {
    var tiles = LoadTestTiles();
    if(tiles == null)
    {
      Trace.TraceInformation(
        $"No tiles found for ID {TileTestGuid}");
      return;
    }
    var icons = new List<BitmapSource>();
    var cache = Host.FileStore.GetIconCache(tiles.Id);
    foreach(var tileBase in tiles.Tiles ?? [])
    {
      var shellLaunch = tileBase?.ShellLaunch;
      if(shellLaunch != null)
      {
        var iconSource = shellLaunch.GetIconSource();
        if(!String.IsNullOrEmpty(shellLaunch.Icon48))
        {
          var cachedIcon = cache.LoadCachedIcon(shellLaunch.Icon48);
          if(cachedIcon != null)
          {
            icons.Add(cachedIcon);
            Trace.TraceInformation(
              $"Found in cache: {shellLaunch.Icon48}: {iconSource}");
            continue;
          }
        }
        var hash = cache.CacheIcon(iconSource, 48, out var icon);
        if(hash == null)
        {
          Trace.TraceError(
            $"Failed to extract and cache icon for {iconSource}");
        }
        else if(icon != null)
        {
          icons.Add(icon);
          Trace.TraceInformation(
            $"Added to cache {hash}: {iconSource}");
        }
        else
        {
          Trace.TraceError(
            $"Confusing result for icon for {iconSource}");
        }
      }
    }
    icons.Reverse();
    if(icons.Count > 0)
    {
      IconSmall = icons[0];
    }
    if(icons.Count > 1)
    {
      IconMedium = icons[1];
    }
    if(icons.Count > 2)
    {
      IconLarge = icons[2];
    }
    if(icons.Count > 3)
    {
      IconExtraLarge = icons[3];
    }
  }

  private void LoadDemoRack()
  {
    var store = Host.Store;
    var rackName = "rack-demo";
    var rack = new RackModel(store, rackName);
    var dbgModel = new Dictionary<string, ShelfData>();
    foreach(var kvp in rack.ShelfMap)
    {
      var shelfKey = kvp.Key.ToString();
      var shelf = kvp.Value;
      var shelfData = shelf.Shelf;
      dbgModel[shelfKey] = shelfData;
    }
    Trace.TraceInformation(
      $"Rack {rackName} loaded with {dbgModel.Count} distinct shelves: " +
      JsonConvert.SerializeObject(dbgModel, Formatting.Indented));
  }
}
