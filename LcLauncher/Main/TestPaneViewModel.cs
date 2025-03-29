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

using Microsoft.Win32;

using Newtonsoft.Json;

using Microsoft.WindowsAPICodePack.Shell;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;
using System.Windows.Media.Imaging;

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
        using(var iconShellFile = ShellFile.FromFilePath(IconFile))
        {
          Trace.TraceInformation(
            $"Parsing Name: {iconShellFile.ParsingName}");
          Trace.TraceInformation(
            $"Display Name: {iconShellFile.Name}");
          var thumbnail = iconShellFile.Thumbnail;
          thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
          IconMedium = thumbnail.MediumBitmapSource;
          IconLarge = thumbnail.LargeBitmapSource;
          IconExtraLarge = thumbnail.ExtraLargeBitmapSource;
          IconNormal = thumbnail.BitmapSource;
          IconSmall = thumbnail.SmallBitmapSource;
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
    IconMedium = null;
    IconLarge = null;
    IconExtraLarge = null;
    IconSmall = null;
    IconNormal = null;
  }

  public BitmapSource? IconMedium {
    get => _iconMedium;
    set {
      if(SetNullableInstanceProperty(ref _iconMedium, value))
      {
      }
    }
  }
  private BitmapSource? _iconMedium;

  public BitmapSource? IconLarge {
    get => _iconLarge;
    set {
      if(SetNullableInstanceProperty(ref _iconLarge, value))
      {
      }
    }
  }
  private BitmapSource? _iconLarge;

  public BitmapSource? IconExtraLarge {
    get => _iconExtraLarge;
    set {
      if(SetNullableInstanceProperty(ref _iconExtraLarge, value))
      {
      }
    }
  }
  private BitmapSource? _iconExtraLarge;

  public BitmapSource? IconSmall {
    get => _iconSmall;
    set {
      if(SetNullableInstanceProperty(ref _iconSmall, value))
      {
      }
    }
  }
  private BitmapSource? _iconSmall;

  public BitmapSource? IconNormal {
    get => _iconNormal;
    set {
      if(SetNullableInstanceProperty(ref _iconNormal, value))
      {
      }
    }
  }
  private BitmapSource? _iconNormal;

  public ICommand ResetShelf1Command { get; }

  public ICommand LoadShelfFileCommand { get; }

  public ICommand OpenIconFileCommand { get; }

  private void ResetShelf1()
  {
    Host.ColumnA.DbgShelfA = new ShelfViewModel(
      Host.ColumnA,
      theme: "Olive",
      title: "Test Shelf");
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
      var data = ShelfData.LoadFile(ofd.FileName);
      var reserialized = JsonConvert.SerializeObject(data, Formatting.Indented);
      Trace.TraceInformation(
        $"Shelf content: {reserialized}");

      // using data is NYI - placeholder
      var shelf = Host.ColumnA.DbgShelfA;
      shelf.Title = data.Title;
      shelf.Theme = data.Theme ?? "Olive";

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
}
