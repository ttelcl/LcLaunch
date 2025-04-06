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

public class QuadTileViewModel: TileViewModelBase
{
  public QuadTileViewModel(List<LaunchTile> model)
  {
    Model = model;
    _model  = model;
    // TODO: expose the 4 tiles in the model explicitly
  }

  public List<LaunchTile> Model {
    get => _model;
    set {
      // Unusually, do not use the property set helpers,
      // but set _model directly.
      _model = value;
      RaisePropertyChanged(nameof(Model));

      //if(SetInstanceProperty(ref _model, value))
      //{
      //  // RaisePropertyChanged(nameof(Model));
      //}
    }
  }
  private List<LaunchTile> _model;

  public override TileData GetModel()
  {
    return TileData.QuadTile(
      Model);
  }
}
