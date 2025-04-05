/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using ControlzEx.Theming;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main;

public class PageColumnViewModel: ViewModelBase
{
  public PageColumnViewModel(
    MainViewModel rootModel)
  {
    RootModel = rootModel;
    _dbgShelfA = new ShelfViewModel(this,
      new ShelfData0("Shelf A", [], theme: "Olive"));
    DbgShelfB = new ShelfViewModel(this,
      new ShelfData0("Shelf B", [], theme: "Amber"));
    DbgShelfC = new ShelfViewModel(this,
      new ShelfData0("Shelf C", [], theme: "Sienna"));
  }

  public MainViewModel RootModel { get; }

  public ICommand SetThemeCommand => new DelegateCommand(
    p => SetTheme(p as string));

  private PageColumn? Host { get; set; }

  public void UpdateHost(PageColumn? host)
  {
    if(host == null)
    {
      Trace.TraceInformation(
        "PageColumn_DataContextChanged: Clearing host control");
      Host = null;
    }
    else
    {
      Trace.TraceInformation(
        "PageColumn_DataContextChanged: Setting host control");
      Host = host;
    }
  }

  public void SetTheme(string? theme)
  {
    if(Host == null)
    {
      Trace.TraceWarning("SetTheme: Host control is null");
      return;
    }
    if(string.IsNullOrEmpty(theme))
    {
      Trace.TraceWarning("SetTheme: Theme is null or empty");
      return;
    }
    ThemeManager.Current.ChangeTheme(Host, theme);
  }

  public ShelfViewModel DbgShelfA { 
    get => _dbgShelfA;
    set {
      if(SetInstanceProperty(ref _dbgShelfA, value))
      {
      }
    }
  }
  private ShelfViewModel _dbgShelfA;

  public ShelfViewModel DbgShelfB { get; }

  public ShelfViewModel DbgShelfC { get; }
}
