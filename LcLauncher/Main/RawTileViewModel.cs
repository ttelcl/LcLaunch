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

namespace LcLauncher.Main;

/// <summary>
/// Description of RawTileViewModel
/// </summary>
public class RawTileViewModel: TileViewModelBase
{
  /// <summary>
  /// Create a new RawTileViewModel
  /// </summary>
  public RawTileViewModel(
    RawLaunch model)
  {
    Model = model;
    _model  = model;
  }

  public RawLaunch Model {
    get => _model;
    set {
      if(SetInstanceProperty(ref _model, value))
      {
        // NOT YET IMPLEMENTED

        //RaisePropertyChanged(nameof(TargetPath));
        //RaisePropertyChanged(nameof(Tooltip));
        //RaisePropertyChanged(nameof(WindowStyle));
        //RaisePropertyChanged(nameof(IconSource));
        //if(String.IsNullOrEmpty(_model.Title))
        //{
        //  Title = Path.GetFileNameWithoutExtension(_model.TargetPath);
        //}
        //else
        //{
        //  Title = _model.Title;
        //}
      }
    }
  }
  private RawLaunch _model;

  public override TileData GetModel()
  {
    return TileData.RawTile(Model);
  }
}
