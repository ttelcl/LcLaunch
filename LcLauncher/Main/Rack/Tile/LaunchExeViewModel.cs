/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using MahApps.Metro.Controls;

using Microsoft.Win32;

namespace LcLauncher.Main.Rack.Tile;

public class LaunchExeViewModel: EditorViewModelBase
{
  public LaunchExeViewModel(
    TileHostViewModel tileHost)
    : base(
        tileHost.Rack.Owner,
        "Executable Launch Tile - Editor",
        tileHost.Shelf.Theme)
  {
    TileHost = tileHost;
    InitCommands();
    Arguments = [];
    Environment = [];
    PathEnvironment = [];
    if(tileHost.Tile is LaunchTileViewModel launchTile)
    {
      if(launchTile.Classification != LaunchKind.Raw)
      {
        throw new InvalidOperationException(
          "Invalid tile type - this constructor expects a tile with a *exe* launch tile");
      }

      var model = launchTile.OldModel;
      if(model is LaunchData launch)
      {
        // Patch to old model
        model = launch.ToRawLaunch();
      }

      if(model is RawLaunch rawLaunch)
      {
        Model = rawLaunch;
        _model = Model;
        Tile = launchTile;
        TargetPath = rawLaunch.TargetPath;
        Title = rawLaunch.Title ?? String.Empty;
        Tooltip = rawLaunch.Tooltip ?? String.Empty;
        WorkingDirectory = rawLaunch.WorkingDirectory ?? String.Empty;
        IconSource = rawLaunch.IconSource ?? String.Empty;
        rawLaunch.Arguments.ForEach(arg => Arguments.Add(arg));
        foreach(var env in rawLaunch.Environment)
        {
          Environment.Add(env.Key, env.Value);
        }
        foreach(var env in rawLaunch.PathEnvironment)
        {
          var edit = new PathEdit(
            env.Value.Prepend,
            env.Value.Append);
          PathEnvironment.Add(env.Key, edit);
        }
      }
      else
      {
        throw new InvalidOperationException(
          "Invalid tile type - this constructor expects a tile with an existing executable launch tile");
      }
    }
    else
    {
      throw new InvalidOperationException(
        "Invalid tile type - this constructor expects a tile with an existing executable launch tile");
    }
  }


  private LaunchExeViewModel(
    TileHostViewModel tileHost,
    RawLaunch newModel)
    : base(
        tileHost.Rack.Owner,
        "Create new Executable Launch tile",
        tileHost.Shelf.Theme)
  {
    TileHost = tileHost;
    InitCommands();
    Model = newModel;
    _model = Model;
    Tile = null;
    TargetPath = newModel.TargetPath;
    Title = newModel.Title ?? String.Empty;
    Tooltip = newModel.Tooltip ?? String.Empty;
    WorkingDirectory = newModel.WorkingDirectory ?? String.Empty;
    IconSource = newModel.IconSource ?? String.Empty;
    // Assume arguments and environments are empty
    Arguments = [];
    Environment = [];
    PathEnvironment = [];
  }

  private void InitCommands()
  {
    PickIconCommand = new DelegateCommand(
      p => PickIconOverride());
    ClearIconCommand = new DelegateCommand(
      p => ClearIconOverride());
    PickWorkingDirectoryCommand = new DelegateCommand(
      p => PickWorkingDirectory());
    ClearWorkingDirectoryCommand = new DelegateCommand(
      p => ClearWorkingDirectory());
  }

  public static LaunchExeViewModel? CreateFromFile(
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
    if(!targetFile.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
    {
      MessageBox.Show(
        $"The selected file is not an executable: {targetFile}",
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

    var rawLaunch = new RawLaunch(
      targetFile,
      Path.GetFileNameWithoutExtension(targetFile),
      Path.GetFileName(targetFile),
      ProcessWindowStyle.Normal,
      iconSource: null,
      icon48: null,
      icon32: null,
      icon16: null,
      workingDirectory: null,
      arguments: null,
      env: null,
      pathenv: null);

    return new LaunchExeViewModel(
      tileHost,
      rawLaunch);
  }

  public ICommand PickIconCommand { get; private set; } = null!;

  public ICommand ClearIconCommand { get; private set; } = null!;

  public ICommand PickWorkingDirectoryCommand { get; private set; } = null!;

  public ICommand ClearWorkingDirectoryCommand { get; private set; } = null!;

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

  public RawLaunch Model {
    get => _model;
    set {
      if(SetInstanceProperty(ref _model, value))
      {
      }
    }
  }
  private RawLaunch _model;

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

  public string WorkingDirectory {
    get => _workingDirectory;
    set {
      if(SetValueProperty(ref _workingDirectory, value))
      {
      }
    }
  }
  private string _workingDirectory = string.Empty;

  public string IconSource {
    get => _iconSource;
    set {
      if(SetValueProperty(ref _iconSource, value))
      {
      }
    }
  }
  private string _iconSource = string.Empty;

  public ObservableCollection<string> Arguments { get; }

  /// <summary>
  /// Edits to the environment variables. Just preserving the original for now.
  /// </summary>
  public Dictionary<string, string?> Environment { get; }

  /// <summary>
  /// Edits to PATH-like environment variables. Just preserving the original for now.
  /// </summary>
  public Dictionary<string, PathEdit> PathEnvironment { get; }

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
    IconSource = String.Empty;
  }

  private void PickWorkingDirectory()
  {
    var dialog = new OpenFolderDialog();
    dialog.Title = "Select a working directory";
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.StartMenu);
    dialog.CustomPlaces.Add(EmptyTileViewModel.CommonStartMenuFolder);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.Desktop);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFiles);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.ProgramFilesCommon);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.Documents);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.LocalApplicationData);
    dialog.CustomPlaces.Add(FileDialogCustomPlaces.RoamingApplicationData);
    dialog.InitialDirectory = Path.GetDirectoryName(TargetPath);
    var result = dialog.ShowDialog();
    if(result != true
      || String.IsNullOrEmpty(dialog.FolderName)
      || !Directory.Exists(dialog.FolderName))
    {
      return;
    }
    WorkingDirectory = dialog.FolderName;
  }

  private void ClearWorkingDirectory()
  {
    WorkingDirectory = string.Empty;
  }

  public override bool CanAcceptEditor()
  {
    if(String.IsNullOrEmpty(TargetPath) 
      || !TargetPath.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
    {
      return false;
    }
    return true;
  }

  public override void AcceptEditor()
  {
    if(CanAcceptEditor())
    {
      var model = Model;
      model.TargetPath = TargetPath;
      model.Title = String.IsNullOrEmpty(Title) ? null : Title;
      model.Tooltip = String.IsNullOrEmpty(Tooltip) ? null : Tooltip;
      model.WorkingDirectory = String.IsNullOrEmpty(WorkingDirectory) ? null : WorkingDirectory;
      model.IconSource = String.IsNullOrEmpty(IconSource) ? null : IconSource;
      model.Arguments.Clear();
      model.Arguments.AddRange(Arguments);
      model.Environment.Clear();
      foreach(var env in Environment)
      {
        model.Environment.Add(env.Key, env.Value);
      }
      model.PathEnvironment.Clear();
      foreach(var env in PathEnvironment)
      {
        var edit = new PathEdit(
          env.Value.Prepend,
          env.Value.Append);
        model.PathEnvironment.Add(env.Key, edit);
      }

      var tile = Tile;
      if(tile is null)
      {
        tile = LaunchTileViewModel.FromRaw(
          TileHost.TileList, model);
        TileHost.Tile = tile;
      }
      else
      {
        // Recreate the tile from the modified model
        tile = LaunchTileViewModel.FromRaw(
          TileHost.TileList, model);
        TileHost.Tile = tile;
      }
      TileHost.TileList.MarkDirty();
      if(String.IsNullOrEmpty(model.Icon48))
      {
        tile.LoadIcon(IconLoadLevel.LoadIfMissing);
      }
      IsActive = false;
    }
  }
}
