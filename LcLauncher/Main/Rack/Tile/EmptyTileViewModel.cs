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

using LcLauncher.WpfUtilities;
using LcLauncher.Main.AppPicker;

using LcLauncher.DataModel.Entities;
using LcLauncher.Main.Rack.Editors;

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
    CreateGroupTileCommand = new DelegateCommand(
      p => CreateGroupTile(),
      p => CanCreateTile());
    CreateShortcutTileCommand = new DelegateCommand(
      p => CreateDocumentTile(true),
      p => CanCreateTile());
    CreateDocumentTileCommand = new DelegateCommand(
      p => CreateDocumentTile(false),
      p => CanCreateTile());
    CreateExecutableTileCommand = new DelegateCommand(
      p => CreateExecutableTile(),
      p => CanCreateTile());
    TryPasteAsTileCommand = new DelegateCommand(
      p => CreateLauncherFromClipboard(),
      p => CanCreateLauncherFromClipboardPrepared());
    CreateAppTileCommand = new DelegateCommand(
      p => CreateAppTile(),
      p => CanCreateTile());
    // Override:
    ClickActionCommand = new DelegateCommand(
      p => DefaultClickCommand(),
      p => CanHandleDefaultClick());
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
      { Launch.ShellMode: true } => "RocketLaunch",
      { Launch.ShellMode: false } => "RocketLaunchOutline",
      { Group: { } } => "DotsGrid",
      { Quad: { } } => "ViewGrid",
      _ => "Egg",
    };
  }

  public ICommand DeleteEmptyTileCommand { get; }

  public ICommand CreateGroupTileCommand { get; }

  public ICommand CreateShortcutTileCommand { get; }

  public ICommand CreateDocumentTileCommand { get; }

  public ICommand CreateExecutableTileCommand { get; }

  public ICommand CreateAppTileCommand { get; }

  public ICommand TryPasteAsTileCommand { get; }

  private void DeleteTile()
  {
    if(Host != null)
    {
      Host.DeleteTile();
    }
  }

  /// <summary>
  /// Handle the click on this tile. Almost the same, as
  /// Swap, but different feedback in case of errors.
  /// </summary>
  private void DefaultClickCommand()
  {
    if(Host == null || Rack.KeyTile == null || Host.IsKeyTile)
    {
      // fail silently for very unusual, unintended, or very common
      // no-ops. All to avoid spam.
      return;
    }
    if(!Host.SwapMarkedTileHereCommand.CanExecute(null))
    {
      MessageBox.Show(
        "Cannot swap that tile here right now",
        "Error");
    }
    else
    {
      Host.SwapMarkedTileHereCommand.Execute(null);
    }
  }

  private bool CanHandleDefaultClick()
  {
    return Host != null && Rack.KeyTile != null;
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

  static readonly Guid CommonStartMenuId =
    Guid.Parse("A4115719-D62E-491D-AA7C-E74B8BE3B067");

  public static FileDialogCustomPlace CommonStartMenuFolder =
    new FileDialogCustomPlace(CommonStartMenuId);

  private static void AddOpenDocumentCustomPlaces(OpenFileDialog dialog)
  {
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.StartMenu);
    dialog.CustomPlaces.Add(CommonStartMenuFolder);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.Desktop);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFiles);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFilesCommon);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.Documents);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.LocalApplicationData);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.RoamingApplicationData);
  }

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
        AddOpenDocumentCustomPlaces(dialog);
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
        AddOpenDocumentCustomPlaces(dialog);
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

    var editModel = LaunchEditViewModel.CreateFromFile(
      Host!,
      fileName,
      true); // we were explicitly asked for a document

    if(editModel == null)
    {
      MessageBox.Show(
        $"Failed to create document tile for {fileName}",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
      return;
    }
    editModel.IsActive = true;
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

    var editModel = LaunchEditViewModel.CreateFromFile(
      Host!,
      fileName,
      false);

    if(editModel == null)
    {
      MessageBox.Show(
        $"Failed to create executable tile for {fileName}",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
      return;
    }
    editModel.IsActive = true;
  }

  private void CreateGroupTile()
  {
    if(CanCreateTile())
    {
      var editModel = new GroupEditViewModel(
        Host!);
      editModel.IsActive = true;
    }
  }

  private void CreateAppTile()
  {
    if(CanCreateTile())
    {
      var selectorModel = Host!.GetAppSelector();
      selectorModel.IsActive = true;
    }
  }

  private void CreateLauncherFromClipboard()
  {
    if(CanCreateTile())
    // allow call even if not yet prepared
    {
      var editModel =
        _preparedClipboardView
        ?? LaunchEditViewModel.TryFromClipboard(Host!, false);
      if(editModel != null)
      {
        // else: a message was shown already
        editModel.IsActive = true;
      }
    }
  }

  private bool CanCreateLauncherFromClipboardPrepared()
  {
    return CanCreateTile() && _preparedClipboardView != null;
  }

  private LaunchEditViewModel? _preparedClipboardView;

  public void PrepareFromClipboard()
  {
    //Trace.TraceError("PrepareFromClipboard() disabled");
    _preparedClipboardView =
      CanCreateTile()
      ? LaunchEditViewModel.TryFromClipboard(Host!, true)
      : null;
    Trace.TraceInformation(
      $"Clipboard tile enable = {_preparedClipboardView != null}");
  }

}
