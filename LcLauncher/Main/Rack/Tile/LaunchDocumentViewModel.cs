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

using LcLauncher.Models;
using LcLauncher.Persistence;

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
        "Document Launch tile editor",
        tileHost.Shelf.Theme)
  {
    TileHost = tileHost;
    if(tileHost.Tile is LaunchTileViewModel launchTile)
    {
      if(launchTile.Classification != LaunchKind.Document)
      {
        throw new InvalidOperationException(
          "Invalid tile type - this constructor expects a tile with a *document* launch tile");
      }

      if(launchTile.Model is ShellLaunch shellLaunch)
      {
        Model = shellLaunch;
        _model = Model;
        Tile = launchTile;
        TargetPath = shellLaunch.TargetPath;
        Title = shellLaunch.Title ?? String.Empty;
        Tooltip = shellLaunch.Tooltip ?? String.Empty;
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
    Model = newModel;
    _model = Model;
    Tile = null;
    TargetPath = newModel.TargetPath;
    Title = newModel.Title ?? String.Empty;
    Tooltip = newModel.Tooltip ?? String.Empty;
  }

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
    model.Title = Title;
    model.Tooltip = Tooltip;
    //model.IconSource = null;
    //model.Icon48 = null;
    //model.Icon32 = null;
    //model.Icon16 = null;
    //model.Verb = string.Empty;
    //model.WindowStyle = ProcessWindowStyle.Normal;
    var tile = Tile;
    if(tile == null)
    {
      tile = LaunchTileViewModel.FromShell(
        TileHost.TileList,
        model);
      TileHost.Tile = tile;
    }
    else
    {
      // force update
      TileHost.Tile = null;
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
