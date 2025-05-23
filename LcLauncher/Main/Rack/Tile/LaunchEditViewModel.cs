﻿/*
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
using LcLauncher.WpfUtilities;

using Microsoft.Win32;

namespace LcLauncher.Main.Rack.Tile;

/// <summary>
/// Launch tile editor ViewModel
/// </summary>
public class LaunchEditViewModel: EditorViewModelBase
{
  // This class merges the old <see cref="LaunchDocumentViewModel"/>
  // and <see cref="LaunchExeViewModel"/> together.

  /// <summary>
  /// Create a new LaunchEditViewModel
  /// </summary>
  public LaunchEditViewModel(
    TileHostViewModel tileHost)
    : base(
        tileHost.Rack.Owner,
        "Launch Tile - Editor",
        tileHost.Shelf.Theme)
  {
    TileHost = tileHost;
    Arguments = [];
    Environment = [];
    PathEnvironment = [];
    InitCommands();
    if(tileHost.Tile is LaunchTileViewModel launchTile)
    {
      var tileModel = launchTile.Model;
      LaunchData? model = null;
      if(tileModel is LaunchData launchData)
      {
        model = launchData;
      }
      else if(tileModel is ShellLaunch shell)
      {
        // convert to new model (LaunchData)
        model = shell.ToLaunch();
      }
      else if(tileModel is RawLaunch raw)
      {
        // convert to new model (LaunchData)
        model = raw.ToLaunch();
      }
      else
      {
        throw new InvalidOperationException(
          "Unrecognized tile model type");
      }
      if(model == null)
      {
        throw new InvalidOperationException(
          "Unrecognized tile model type -- internal error");
      }
      Model = model;
      Tile = launchTile;
      IsShellMode = model.ShellMode;
      Target = model.Target; // indirectly sets Classification
      Title = model.Title ?? String.Empty;
      Tooltip = model.Tooltip ?? String.Empty;
      Verb = model.Verb;
      IconSource = model.IconSource ?? String.Empty;
      WorkingDirectory = model.WorkingDirectory ?? String.Empty;
      model.Arguments.ForEach(arg => Arguments.Add(arg));
      foreach(var env in model.Environment)
      {
        Environment.Add(env.Key, env.Value);
      }
      foreach(var env in model.PathEnvironment)
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
        "Invalid tile type - this constructor expects a tile with an existing launch tile");
    }
  }

  private LaunchEditViewModel(
    TileHostViewModel tileHost,
    LaunchData model)
    : base(
        tileHost.Rack.Owner,
        "Create new Launch Tile - Editor",
        tileHost.Shelf.Theme)
  {
    TileHost = tileHost;
    Arguments = [];
    Environment = [];
    PathEnvironment = [];
    InitCommands();
    Model = model;
    Tile = null;
    IsShellMode = model.ShellMode;
    Target = model.Target; // indirectly sets Classification
    Title = model.Title ?? String.Empty;
    Tooltip = model.Tooltip ?? String.Empty;
    Verb = model.Verb;
    IconSource = model.IconSource ?? String.Empty;
    WorkingDirectory = model.WorkingDirectory ?? String.Empty;
    // Assume that the input model does not have any arguments,
    // environment variables, or path variable edits.
  }

  public static  LaunchEditViewModel CreateEmpty(
    TileHostViewModel tileHost)
  {
    var model = new LaunchData(
      String.Empty,
      true);
    return new LaunchEditViewModel(tileHost, model);
  }

  public static LaunchEditViewModel CreateFromFile(
    TileHostViewModel tileHost,
    string targetFile)
  {
    var isExe = targetFile.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
    var model = new LaunchData(
      targetFile,
      !isExe,
      Path.GetFileNameWithoutExtension(targetFile),
      Path.GetFileName(targetFile));
    return new LaunchEditViewModel(tileHost, model);
  }

  public static LaunchEditViewModel CreateFromApp(
    TileHostViewModel tileHost,
    string applicationName,
    string applicationIdentifier)
  {
    if(!LaunchData.HasShellAppsFolderPrefix(applicationIdentifier))
    {
      applicationIdentifier =
        LaunchData.ShellAppsFolderPrefix + applicationIdentifier;
    }
    var model = new LaunchData(
      applicationIdentifier,
      true,
      applicationName,
      null);
    return new LaunchEditViewModel(tileHost, model);
  }

  public ICommand PickIconCommand { get; private set; } = null!;

  public ICommand ClearIconCommand { get; private set; } = null!;

  public ICommand PickWorkingDirectoryCommand { get; private set; } = null!;

  public ICommand ClearWorkingDirectoryCommand { get; private set; } = null!;

  private void InitCommands()
  {
    PickIconCommand = new DelegateCommand(
      p => PickIconOverride());
    ClearIconCommand = new DelegateCommand(
      p => ClearIconOverride());
    PickWorkingDirectoryCommand = new DelegateCommand(
      p => PickWorkingDirectory(),
      p => AllowWorkingDirectory());
    ClearWorkingDirectoryCommand = new DelegateCommand(
      p => ClearWorkingDirectory());
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

  public LaunchData Model { get; }

  public LaunchKind Classification {
    get => _classification;
    set {
      if(SetValueProperty(ref _classification, value))
      {
      }
    }
  }
  private LaunchKind _classification = LaunchKind.Invalid;

  public string Title {
    get => _title;
    set {
      if(SetValueProperty(ref _title, value))
      {
      }
    }
  }
  private string _title = string.Empty;

  public string Target {
    get => _target;
    set {
      if(SetValueProperty(ref _target, value))
      {
        Classification = LaunchData.GetLaunchKind(value, !Model.ShellMode);
      }
    }
  }
  private string _target = string.Empty;

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

  public string WorkingDirectory {
    get => _workingDirectory;
    set {
      if(SetValueProperty(ref _workingDirectory, value))
      {
      }
    }
  }
  private string _workingDirectory = string.Empty;

  public bool IsShellMode {
    get => _isShellMode;
    set {
      if(SetValueProperty(ref _isShellMode, value))
      {
        Classification = LaunchData.GetLaunchKind(
          Target, !value);
      }
    }
  }
  private bool _isShellMode = false;

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

    // The following is no longer valid: 'Target' may be something else than a path now
    //dialog.InitialDirectory = Path.GetDirectoryName(TargetPath);

    var result = dialog.ShowDialog();
    if(result != true
      || String.IsNullOrEmpty(dialog.FolderName)
      || !Directory.Exists(dialog.FolderName))
    {
      return;
    }
    WorkingDirectory = dialog.FolderName;
  }

  private bool AllowWorkingDirectory()
  {
    return Classification == LaunchKind.Raw; 
  }

  private void ClearWorkingDirectory()
  {
    WorkingDirectory = string.Empty;
  }

  public string? WhyNotValid()
  {
    if(String.IsNullOrEmpty(Target))
    {
      return "The target is empty";
    }
    if(Classification == LaunchKind.Invalid)
    {
      return "The target is not in any valid form";
    }
    if(IsShellMode)
    {
      // some things are not allowed in shell mode
      if(
        !String.IsNullOrEmpty(WorkingDirectory)
        || Environment.Count != 0
        || PathEnvironment.Count != 0)
      {
        return "In shell mode, WorkingDirectory, Environment, and PathEnvironment are not allowed";
      }
      if(
        Classification == LaunchKind.Document
        && !File.Exists(Target))
      {
        return "The target looks like a file name, but does not exist";
      }
      if(Classification == LaunchKind.Raw)
      {
        return "(internal error - Raw classification in Shell mode)";
      }
    }
    else
    {
      // Some things are not allowed in raw mode
      if(!String.IsNullOrEmpty(Verb))
      {
        return "'Verb' is only allowed in shell mode";
      }
      if(!Target.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
      {
        return "Non-shell mode requires a target that is an executable file";
      }
      if(!File.Exists(Target))
      {
        return "The target file does not exist";
      }
      if(Classification != LaunchKind.Raw)
      {
        return "(internal error - non-raw in non-shell mode)";
      }
    }
    // I probably missed something, but anyway :)
    return null;
  }

  public override bool CanAcceptEditor()
  {
    return WhyNotValid() == null;
  }

  public override void AcceptEditor()
  {
    var reason = WhyNotValid();
    if(reason == null)
    {
      // valid
      // NYI
      MessageBox.Show(
        "This editor is not yet implemented",
        "Not implemented",
        MessageBoxButton.OK,
        MessageBoxImage.Warning);
    }
    else
    {
      MessageBox.Show(
        reason,
        "Invalid values",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
    }
  }
}
