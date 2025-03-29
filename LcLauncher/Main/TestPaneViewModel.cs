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
      if(_iconFile != null && _iconFile != value && value != null)
      {
        SetValueProperty(ref _iconFile, null);
      }
      if(SetValueProperty(ref _iconFile, value))
      {
      }
    }
  }
  private string? _iconFile;

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
    };
    var result = ofd.ShowDialog();
    if(result == true)
    {
      IconFile = ofd.FileName;
    }
  }
}
