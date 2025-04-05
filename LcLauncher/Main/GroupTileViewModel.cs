/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LcLauncher.Models;

namespace LcLauncher.Main;

public class GroupTileViewModel: TileViewModelBase
{
  public GroupTileViewModel(
    TileGroup0 model)
  {
    _model = model;
  }

  public TileGroup0 Model {
    get => _model;
    set {
      if(SetInstanceProperty(ref _model, value))
      {
        RaisePropertyChanged(nameof(Title));
        RaisePropertyChanged(nameof(Tiles));
      }
    }
  }
  private TileGroup0 _model;

  public string Title => Model.Title;

  public IList<TileData0?> Tiles => Model.Tiles;

  public override TileData0 GetModel()
  {
    throw new NotImplementedException();
  }
}
