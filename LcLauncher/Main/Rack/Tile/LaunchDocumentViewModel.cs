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

using LcLauncher.Models;
using LcLauncher.Persistence;
using LcLauncher.WpfUtilities;

using Microsoft.Win32;

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// Edit or create a document launcher tile (which is
/// one kind of shell mode launcher)
/// </summary>
public class LaunchDocumentViewModel: EditorViewModelBase
{

  /// <summary>
  /// Create a new LaunchEditorViewModel
  /// </summary>
  public LaunchDocumentViewModel(
    TileHostViewModel tileHost)
    : base(
        tileHost.Rack.Owner,
        "Document Launch Tile - Editor",
        tileHost.Shelf.Theme)
  {
    TileHost = tileHost;
    PickIconCommand = new DelegateCommand(
      p => PickIconOverride());
    ClearIconCommand = new DelegateCommand(
      p => ClearIconOverride());
    if(tileHost.Tile is LaunchTileViewModel launchTile)
    {
      if(launchTile.Classification != LaunchKind.Document)
      {
        throw new InvalidOperationException(
          "Invalid tile type - this constructor expects a tile with a *document* launch tile");
      }

      var model = launchTile.OldModel;

      if(model is LaunchData launchData)
      {
        // Patch to old model
        model = launchData.ToShellLaunch();
      }

      if(model is ShellLaunch shellLaunch)
      {
        Model = shellLaunch;
        _model = Model;
        Tile = launchTile;
        TargetPath = shellLaunch.TargetPath;
        Title = shellLaunch.Title ?? String.Empty;
        Tooltip = shellLaunch.Tooltip ?? String.Empty;
        Verb = shellLaunch.Verb;
        IconSource = shellLaunch.IconSource ?? String.Empty;
      }
      else
      {
        throw new InvalidOperationException(
          "Invalid tile type - this constructor expects a tile with an existing document launch tile");
      }
    }
    else
    {
      throw new InvalidOperationException(
        "Invalid tile type - this constructor expects a tile with an existing document launch tile");
    }
  }

  private LaunchDocumentViewModel(
    TileHostViewModel tileHost,
    ShellLaunch newModel)
    : base(
        tileHost.Rack.Owner,
        "Create new Document Launch tile",
        tileHost.Shelf.Theme)
  {
    TileHost = tileHost;
    PickIconCommand = new DelegateCommand(
      p => PickIconOverride());
    ClearIconCommand = new DelegateCommand(
      p => ClearIconOverride());
    Model = newModel;
    _model = Model;
    Tile = null;
    TargetPath = newModel.TargetPath;
    Title = newModel.Title ?? String.Empty;
    Tooltip = newModel.Tooltip ?? String.Empty;
    IconSource = newModel.IconSource ?? String.Empty;
    Verb = newModel.Verb;
  }

  public ICommand PickIconCommand { get; }

  public ICommand ClearIconCommand { get; }

  public static LaunchDocumentViewModel? CreateFromFile(
    TileHostViewModel tileHost,
    string targetFile)
  {
    if(!File.Exists(targetFile))
    {
      MessageBox.Show(
        $"File does not exist: {targetFile}",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
      return null;
    }
    targetFile = Path.GetFullPath(targetFile);
    if(tileHost.Tile is not EmptyTileViewModel)
    {
      MessageBox.Show(
        "Tile is not empty",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
      return null;
    }

    var shellLaunch = new ShellLaunch(
      targetFile,
      Path.GetFileNameWithoutExtension(targetFile),
      Path.GetFileName(targetFile),
      ProcessWindowStyle.Normal,
      iconSource: null,
      icon48: null,
      icon32: null,
      icon16: null,
      verb: string.Empty);

    return new LaunchDocumentViewModel(
      tileHost,
      shellLaunch);
  }

  public TileHostViewModel TileHost { get; }

  /// <summary>
  /// Store intermediate data for the tile.
  /// Can be the original when editing or null
  /// for a brand new tile.
  /// </summary>
  public LaunchTileViewModel? Tile {
    get => _tile;
    set {
      if(SetNullableInstanceProperty(ref _tile, value))
      {
      }
    }
  }
  private LaunchTileViewModel? _tile = null;

  public ShelfViewModel Shelf => TileHost.Shelf;

  public ShellLaunch Model {
    get => _model;
    set {
      if(SetInstanceProperty(ref _model, value))
      {
      }
    }
  }
  private ShellLaunch _model;

  public string Title {
    get => _title;
    set {
      if(SetValueProperty(ref _title, value))
      {
      }
    }
  }
  private string _title = string.Empty;

  public string TargetPath {
    get => _targetPath;
    set {
      if(SetValueProperty(ref _targetPath, value))
      {
      }
    }
  }
  private string _targetPath = string.Empty;

  public string Tooltip {
    get => _tooltip;
    set {
      if(SetValueProperty(ref _tooltip, value))
      {
      }
    }
  }
  private string _tooltip = string.Empty;

  public string IconSource {
    get => _iconSource;
    set {
      if(SetValueProperty(ref _iconSource, value))
      {
      }
    }
  }
  private string _iconSource = string.Empty;

  public string Verb {
    get => _verb;
    set {
      if(SetValueProperty(ref _verb, value))
      {
      }
    }
  }
  private string _verb = string.Empty;

  private void PickIconOverride()
  {
    var dialog = new OpenFileDialog();
    dialog.Filter = "Any file (*.*)|*.*";
    dialog.Title = "Select a file to derive the tile icon from";
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.StartMenu);
    dialog.CustomPlaces.Add(EmptyTileViewModel.CommonStartMenuFolder);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.Desktop);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFiles);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFilesCommon);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.Documents);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.LocalApplicationData);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.RoamingApplicationData);
    dialog.DereferenceLinks = false;
    var result = dialog.ShowDialog();
    if(result != true
      || String.IsNullOrEmpty(dialog.FileName)
      || !File.Exists(dialog.FileName))
    {
      return;
    }
    IconSource = dialog.FileName;
  }

  private void ClearIconOverride()
  {
    IconSource = string.Empty;
  }

  public override bool CanAcceptEditor()
  {
    if(String.IsNullOrEmpty(TargetPath))
    {
      return false;
    }
    return true;
  }

  public override void AcceptEditor()
  {
    var model = Model;
    model.TargetPath = TargetPath;
    model.Title = String.IsNullOrEmpty(Title) ? null : Title;
    model.Tooltip = String.IsNullOrEmpty(Tooltip) ? null : Tooltip;
    model.IconSource = String.IsNullOrEmpty(IconSource) ? null : IconSource;
    //model.Icon48 = null;
    //model.Icon32 = null;
    //model.Icon16 = null;
    model.Verb = Verb;
    //model.WindowStyle = ProcessWindowStyle.Normal;

    // Create a new tile or recreate the modified one
    var tile = LaunchTileViewModel.FromShell(
      TileHost.TileList,
      model);
    TileHost.Tile = tile;
    TileHost.TileList.MarkDirty();
    if(String.IsNullOrEmpty(model.Icon48))
    {
      tile.LoadIcon(IconLoadLevel.LoadIfMissing);
    }
    IsActive = false;
  }
}
