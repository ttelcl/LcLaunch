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
using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack.Tile;

public class EmptyTileViewModel: TileViewModel
{
  public EmptyTileViewModel(
    TileListViewModel ownerList,
    TileData? model,
    string? icon = null)
    : base(ownerList)
  {
    Model = model;
    Icon = icon ?? FindIcon();
    DeleteEmptyTileCommand = new DelegateCommand(
      p => DeleteTile(),
      p => CanDeleteTile());
    CreateShortcutTileCommand = new DelegateCommand(
      p => CreateDocumentTile(true),
      p => CanCreateTile());
    CreateDocumentTileCommand = new DelegateCommand(
      p => CreateDocumentTile(false),
      p => CanCreateTile());
    CreateExecutableTileCommand = new DelegateCommand(
      p => CreateExecutableTile(),
      p => CanCreateTile());
  }

  /// <summary>
  /// The original model this tile view model was created from,
  /// possibly null. Unlike other tiles, this is immutable.
  /// </summary>
  public TileData? Model { get; }

  public override TileData? GetModel()
  {
    return TileData.EmptyTile();
  }

  public string Icon {
    get => _icon;
    set {
      if(SetValueProperty(ref _icon, value))
      {
      }
    }
  }
  private string _icon = "Egg";

  public override string PlainIcon { get => Icon; }

  private string FindIcon()
  {
    return Model switch {
      null => "EggOutline",
      { ShellLaunch: { } } => "RocketLaunch",
      { RawLaunch: { } } => "RocketLaunchOutline",
      { Group: { } } => "DotsGrid",
      { Quad: { } } => "ViewGrid",
      _ => "Egg",
    };
  }

  public ICommand DeleteEmptyTileCommand { get; }

  public ICommand CreateShortcutTileCommand { get; }

  public ICommand CreateDocumentTileCommand { get; }

  public ICommand CreateExecutableTileCommand { get; }

  private void DeleteTile()
  {
    if(Host != null)
    {
      Host.DeleteTile();
    }
  }

  private bool CanDeleteTile()
  {
    return Host != null && !Host.IsKeyTile;
  }

  private bool CanCreateTile()
  {
    return
      Host != null
      && Host.Rack.KeyTile == null
      && Host.Rack.KeyShelf == null;
  }

  //static Guid AppsFolderId = Guid.Parse("1e87508d-89c2-42f0-8a7e-645a0f50ca58");
  static Guid CommonStartMenuId = Guid.Parse("A4115719-D62E-491D-AA7C-E74B8BE3B067");
  static FileDialogCustomPlace CommonStartMenuFolder =
    new FileDialogCustomPlace(CommonStartMenuId);

  /// <summary>
  /// Create a new launch tile and replace this empty tile.
  /// This variant creates a "shell mode" launch tile that
  /// can launch any document.
  /// </summary>
  /// <param name="shortcutMode">
  /// If true, the target is assumed to be a shortcut, and the
  /// OpenfileDialog will be set up accordingly.
  /// </param>
  private void CreateDocumentTile(
    bool shortcutMode)
  {
    if(CanCreateTile())
    {
      var dialog = new OpenFileDialog();
      if(shortcutMode)
      {
        dialog.Filter = "Shortcut files (*.lnk)|*.lnk";
        dialog.Title = "Select a shortcut file";
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.StartMenu);
        dialog.CustomPlaces.Add(CommonStartMenuFolder);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.Desktop);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFiles);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFilesCommon);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.Documents);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.LocalApplicationData);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.RoamingApplicationData);
        dialog.InitialDirectory =
          Environment.GetFolderPath(
            Environment.SpecialFolder.StartMenu);
        dialog.DereferenceLinks = false;
        dialog.ShowHiddenItems = true;
      }
      else
      {
        dialog.Filter = "All files (*.*)|*.*|Executables (*.exe)|*.exe";
        dialog.Title = "Select a document file (shortcuts are dereferenced to their target)";
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.StartMenu);
        dialog.CustomPlaces.Add(CommonStartMenuFolder);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.Desktop);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFiles);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFilesCommon);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.Documents);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.LocalApplicationData);
        dialog.CustomPlaces.Add(FileDialogCustomPlaces.RoamingApplicationData);
        dialog.InitialDirectory =
          Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments);
        dialog.DereferenceLinks = true;
        dialog.ShowHiddenItems = true;
      }
      dialog.CheckPathExists = true;
      dialog.CheckFileExists = true;
      dialog.Multiselect = false;
      dialog.FilterIndex = 0;

      var result = dialog.ShowDialog();
      if(result != true
        || String.IsNullOrEmpty(dialog.FileName)
        || !File.Exists(dialog.FileName))
      {
        return;
      }
      var target = dialog.FileName;
      if(target.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
      {
        var result2 = MessageBox.Show(
          "The selected file is an executable.\n"+
          "Do you want to create a non-shell launch tile for it? \n" +
          "(this enables options not available to shell launch tiles)",
          "Executable file",
          MessageBoxButton.YesNoCancel,
          MessageBoxImage.Question);
        if(result2 == MessageBoxResult.Cancel)
        {
          return;
        }
        if(result2 == MessageBoxResult.Yes)
        {
          CreateExecutableTileFor(target);
          return;
        }
        // fall through
      }
      CreateDocumentTileFor(target);
    }
  }

  private void CreateExecutableTile()
  {
    if(CanCreateTile())
    {
      var dialog = new OpenFileDialog();
      dialog.Filter = "Executables (*.exe)|*.exe";
      dialog.Title = "Select an executable file";
      dialog.CustomPlaces.Add(FileDialogCustomPlaces.Desktop);
      dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFiles);
      dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFilesCommon);
      var result = dialog.ShowDialog();
      if(result != true
        || String.IsNullOrEmpty(dialog.FileName)
        || !File.Exists(dialog.FileName))
      {
        return;
      }
      var target = dialog.FileName;
      if(!target.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
      {
        MessageBox.Show(
          "The selected file is not an executable.",
          "Error",
          MessageBoxButton.OK,
          MessageBoxImage.Error);
        return;
      }
      CreateExecutableTileFor(target);
    }
  }

  private void CreateDocumentTileFor(string fileName)
  {
    fileName = Path.GetFullPath(fileName);
    if(!File.Exists(fileName))
    {
      MessageBox.Show(
        $"File does not exist: {fileName}",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
      return;
    }
    Trace.TraceInformation(
      $"Creating DOCUMENT tile for: '{Path.GetFileName(fileName)}' ({fileName})");
    MessageBox.Show(
      $"Not yet implemented: CreateDocumentTileFor('{Path.GetFileName(fileName)}')",
      "Work in progress",
      MessageBoxButton.OK,
      MessageBoxImage.Warning);
  }

  private void CreateExecutableTileFor(string fileName)
  {
    fileName = Path.GetFullPath(fileName);
    if(!File.Exists(fileName))
    {
      MessageBox.Show(
        $"File does not exist: {fileName}",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
      return;
    }
    Trace.TraceInformation(
      $"Creating EXECUTABLE tile for: '{Path.GetFileName(fileName)}' ({fileName})");
    MessageBox.Show(
      $"Not yet implemented: CreateExecutableTileFor('{Path.GetFileName(fileName)}')",
      "Work in progress",
      MessageBoxButton.OK,
      MessageBoxImage.Warning);
  }
}
