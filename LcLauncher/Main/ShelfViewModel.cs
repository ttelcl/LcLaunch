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
using LcLauncher.Models;

namespace LcLauncher.Main;

public class ShelfViewModel: ViewModelBase
{
  public ShelfViewModel(
    PageColumnViewModel columnModel,
    ShelfData shelfData)
  {
    ColumnModel = columnModel;
    SetThemeCommand = new DelegateCommand(
      p => Theme = (p as string) ?? "Olive");
    ToggleExpandedCommand = new DelegateCommand(
      p => IsExpanded = !IsExpanded);
    PrimaryContent = new ShelfContentViewModel(this, true);
    ShelfData = shelfData;
    _shelfData = shelfData; // make the compiler shut up
    //Theme = theme ?? "Olive";
    _title = String.IsNullOrEmpty(ShelfData.Title)
      ? "Untitled Shelf"
      : ShelfData.Title;
    PrimaryContent.MapTiles();
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

  public ShelfData ShelfData {
    get => _shelfData;
    set {
      if(SetInstanceProperty(ref _shelfData, value))
      {
        Title = value.Title;
        Theme = value.Theme ?? "Olive";
        PrimaryContent.MapTiles();
      }
    }
  }
  private ShelfData _shelfData;

  public string Theme {
    get => _theme;
    set {
      if(SetValueProperty(ref _theme, value))
      {
        if(_shelfData.Theme != value)
        {
          _shelfData.Theme = value;
        }
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
        if(_shelfData.Title != value)
        {
          _shelfData.Title = value;
        }
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
        "ShelfViewModel.UpdateHost: Clearing host control");
      Host = null;
    }
    else
    {
      Trace.TraceInformation(
        "ShelfViewModel.UpdateHost: Setting host control");
      Host = host;
      SetTheme("Dark." + Theme);
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
