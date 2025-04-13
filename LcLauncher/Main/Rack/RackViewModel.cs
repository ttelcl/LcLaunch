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
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class RackViewModel: ViewModelBase
{
  public RackViewModel(
    MainViewModel owner,
    RackModel model)
  {
    Owner = owner;
    Model = model;
  }

  public MainViewModel Owner { get; }

  public RackModel Model { get; }

  public ILcLaunchStore Store => Model.Store;

  public string Name => Model.RackName;
}
