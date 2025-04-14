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
    OpenIconFileCommand = new DelegateCommand(p => OpenIconFile());
    TestRebuildCommand = new DelegateCommand(p => TestRebuild());
  }

  public MainViewModel Host { get; }

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

  public ICommand OpenIconFileCommand { get; }

  public ICommand TestRebuildCommand { get; }

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

  private void TestRebuild()
  {
    var testShelfId = new Guid("4bfb0220-9c04-4456-a2a9-fa9d870850fe");
    var copyId = new Guid("bef10af3-a1f7-4950-97ee-a9c23305b371");
    var rack = Host.CurrentRack;
    if(rack == null)
    {
      Trace.TraceError(
        $"TestRebuild: No rack selected");
      return;
    }
    var shelfVm =
      rack.AllShelves()
      .FirstOrDefault(vm => vm.Model.Id == testShelfId);
    if(shelfVm == null)
    {
      Trace.TraceError(
        $"TestRebuild: No shelf found with ID {testShelfId}");
      return;
    }
    Trace.TraceInformation(
      $"TestRebuild: Found shelf {testShelfId}");
    var primaryTiles = shelfVm.PrimaryTiles;
    Trace.TraceInformation(
      $"TestRebuild: Rebuilding tiles and saving a copy");
    primaryTiles.RebuildModel();
    primaryTiles.Model.DevSaveCopy(copyId);
  }

}
