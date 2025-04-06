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
using System.Windows.Media.Imaging;

using LcLauncher.Models;

namespace LcLauncher.Main;

/// <summary>
/// View model for a tile wrapping a <see cref="ShellLaunch"/>
/// </summary>
public class ShellTileViewModel: TileViewModelBase
{
  /// <summary>
  /// Create a new ShellTileViewModel
  /// </summary>
  public ShellTileViewModel(
    ShellLaunch model)
  {
    Model = model;
    _model  = model;
  }

  public ShellLaunch Model {
    get => _model;
    set {
      if(SetInstanceProperty(ref _model, value))
      {
        RaisePropertyChanged(nameof(TargetPath));
        RaisePropertyChanged(nameof(Tooltip));
        RaisePropertyChanged(nameof(WindowStyle));
        RaisePropertyChanged(nameof(Verb));
        RaisePropertyChanged(nameof(IconSource));
        if(String.IsNullOrEmpty(_model.Title))
        {
          Title = Path.GetFileNameWithoutExtension(_model.TargetPath);
        }
        else
        {
          Title = _model.Title;
        }
      }
    }
  }
  private ShellLaunch _model;

  public string TargetPath => Model.TargetPath;

  public string? Tooltip => Model.Tooltip;

  public string Title {
    get => _title;
    set {
      if(SetInstanceProperty(ref _title, value))
      {
        RaisePropertyChanged(nameof(Title));
      }
    }
  }
  private string _title = "";

  public ProcessWindowStyle WindowStyle => Model.WindowStyle;

  public string? IconSource => Model.IconSource;

  public string? Verb => Model.Verb;

  public override TileData GetModel()
  {
    return TileData.ShellTile(Model);
  }

  public BitmapSource? IconLarge {
    get => _iconLarge;
    set {
      if(SetNullableInstanceProperty(ref _iconLarge, value))
      {
        RaisePropertyChanged(nameof(IconLarge));
      }
    }
  }
  private BitmapSource? _iconLarge;
}