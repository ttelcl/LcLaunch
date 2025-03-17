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
using ControlzEx.Theming;
using System.Windows.Input;

namespace LcLauncher.Main;

public class ShelfViewModel: ViewModelBase
{
  public ShelfViewModel(
    PageColumnViewModel columnModel)
  {
    ColumnModel = columnModel;
  }

  public PageColumnViewModel ColumnModel { get; }

  public MainViewModel RootModel => ColumnModel.RootModel;

  public ICommand SetThemeCommand => new DelegateCommand(
    p => SetTheme(p as string));

  public bool IsExpanded {
    get => _isExpanded;
    set {
      if(SetValueProperty(ref _isExpanded, value))
      {
      }
    }
  }
  private bool _isExpanded = true;

  public int RowCount {
    get => _rowCount;
    set {
      if(SetValueProperty(ref _rowCount, value))
      {
      }
    }
  }
  private int _rowCount = 1;

  private Shelf? Host { get; set; }

  public void UpdateHost(Shelf? host)
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

}
