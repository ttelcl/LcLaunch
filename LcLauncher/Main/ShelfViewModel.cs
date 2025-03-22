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
    SetThemeCommand = new DelegateCommand(
      p => Theme = (p as string) ?? "Olive");
    PrimaryContent = new ShelfContentViewModel(this, true);
  }

  public PageColumnViewModel ColumnModel { get; }

  public MainViewModel RootModel => ColumnModel.RootModel;

  public ShelfContentViewModel PrimaryContent { get; }

  public ShelfContentViewModel? SecondaryContent {
    get => _secondaryContent;
    set {
      if(SetNullableInstanceProperty(ref _secondaryContent, value))
      {
        RaisePropertyChanged(nameof(HasSecondaryContent));
      }
    }
  }
  private ShelfContentViewModel? _secondaryContent;

  public bool HasSecondaryContent => SecondaryContent != null;

  public ICommand SetThemeCommand { get; }

  public string Theme {
    get => _theme;
    set {
      if(SetValueProperty(ref _theme, value))
      {
        SetTheme("Dark." + value);
      }
    }
  }
  private string _theme = "Olive";

  public bool IsExpanded {
    get => _isExpanded;
    set {
      if(SetValueProperty(ref _isExpanded, value))
      {
      }
    }
  }
  private bool _isExpanded = true;

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

  private void SetTheme(string? theme)
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
