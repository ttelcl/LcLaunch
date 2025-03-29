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
    TileGroup model)
  {
    _model = model;
  }

  public TileGroup Model {
    get => _model;
    set {
      if(SetInstanceProperty(ref _model, value))
      {
        RaisePropertyChanged(nameof(Title));
        RaisePropertyChanged(nameof(Tiles));
      }
    }
  }
  private TileGroup _model;

  public string Title => Model.Title;

  public IList<TileData?> Tiles => Model.Tiles;

  public override TileData GetModel()
  {
    throw new NotImplementedException();
  }
}
