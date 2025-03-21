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

namespace LcLauncher.Main;

public class ShelfContentViewModel: ViewModelBase
{
  public ShelfContentViewModel(
    ShelfViewModel owner)
  {
    Owner = owner;
  }

  public ShelfViewModel Owner { get; }

}
