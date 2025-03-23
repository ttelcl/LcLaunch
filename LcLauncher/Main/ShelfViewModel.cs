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
    ToggleExpandedCommand = new DelegateCommand(
      p => IsExpanded = !IsExpanded);
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
      }
    }
  }
  private ShelfContentViewModel? _secondaryContent;

  public void ToggleSecondaryContent(ShelfContentViewModel? content)
  {
    if(content == null || Object.ReferenceEquals(content, SecondaryContent))
    {
      SecondaryContent = null;
    }
    else
    {
      SecondaryContent = content;
    }
  }

  public ICommand SetThemeCommand { get; }

  public ICommand ToggleExpandedCommand { get; }

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

  public string Title {
    get => _title;
    set {
      if(SetValueProperty(ref _title, value))
      {
      }
    }
  }
  private string _title = "Shelf Title";

  public bool IsExpanded {
    get => _isExpanded;
    set {
      if(SetValueProperty(ref _isExpanded, value))
      {
        RaisePropertyChanged(nameof(ShelfExpandedIcon));
      }
    }
  }
  private bool _isExpanded = true;

  public string ShelfExpandedIcon => 
    IsExpanded ? "ChevronUpCircleOutline" : "ChevronDownCircleOutline";

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
