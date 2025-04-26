/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.Models;

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
        _tile = Tile;
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

  public TileHostViewModel TileHost { get; }

  /// <summary>
  /// Store intermediate data for the tile.
  /// </summary>
  public LaunchTileViewModel Tile {
    get => _tile;
    set {
      if(SetInstanceProperty(ref _tile, value))
      {
      }
    }
  }
  private LaunchTileViewModel _tile;

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

}
