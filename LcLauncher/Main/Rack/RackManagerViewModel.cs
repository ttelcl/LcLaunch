/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class RackManagerViewModel: ViewModelBase
{
  public RackManagerViewModel(
    MainViewModel main)
  {
    Main = main;
  }

  public MainViewModel Main { get; }

  public RackListViewModel RackList => Main.RackList;
}
