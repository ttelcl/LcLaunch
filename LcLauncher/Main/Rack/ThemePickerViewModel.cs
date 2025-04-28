/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using MahApps.Metro.Converters;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

/// <summary>
/// Theme picker. Owns a list of <see cref="ThemeWidgetViewModel"/>s
/// and tracks a 'current' one.
/// </summary>
public class ThemePickerViewModel: ViewModelBase
{
  /// <summary>
  /// Create a new ThemePickerViewModel
  /// </summary>
  public ThemePickerViewModel(
    string defaultThemeName,
    IHasTheme? destination)
  {
    DefaultThemeName = defaultThemeName;
    ThemeWidgets = [];
    InitializeThemes();
    Destination = destination;
    if(Destination != null)
    {
      // try to set the current theme
      CurrentTheme = Destination.Theme;
    }
  }

  public ObservableCollection<ThemeWidgetViewModel> ThemeWidgets { get; }

  public IHasTheme? Destination { get; }

  public ThemeWidgetViewModel? CurrentThemeWidget {
    get => _currentThemeWidget;
    set {
      var oldThemeWidget = _currentThemeWidget;
      if(SetNullableInstanceProperty(ref _currentThemeWidget, value))
      {
        if(oldThemeWidget != null)
        {
          oldThemeWidget.IsCurrent = false;
        }
        if(_currentThemeWidget != null)
        {
          _currentThemeWidget.IsCurrent = true;
          Trace.TraceInformation(
            $"Theme changed to '{_currentThemeWidget.Theme}'");
        }
        else
        {
          Trace.TraceInformation(
            $"Picker Theme reset to NULL");
        }
        RaisePropertyChanged(nameof(CurrentTheme));
        if(Destination != null)
        { 
          Destination.Theme = 
            _currentThemeWidget?.Theme
            ?? DefaultTheme?.Theme
            ?? DefaultThemeName;
        }
      }
    }
  }
  private ThemeWidgetViewModel? _currentThemeWidget;

  public string? CurrentTheme {
    get => _currentThemeWidget?.Theme;
    set {
      if(CurrentTheme != value)
      {
        if(value == null)
        {
          CurrentThemeWidget = null;
        }
        else
        {
          var themeWidget = ThemeWidgets.FirstOrDefault(
            tw => StringComparer.InvariantCultureIgnoreCase.Equals(tw.Theme, value));
          if(themeWidget != null)
          {
            CurrentThemeWidget = themeWidget;
          }
          else
          {
            Trace.TraceError(
              $"Unknown theme '{value}'. Using fallback");
            CurrentThemeWidget = DefaultTheme;
          }
        }
      }
    }
  }

  public string DefaultThemeName { get; }

  public ThemeWidgetViewModel? DefaultTheme {
    get =>
      ThemeWidgets.FirstOrDefault(twvm => twvm.Theme == DefaultThemeName)
      ?? ThemeWidgets.FirstOrDefault();
  }

  private void AddTheme(
    string name,
    Brush sampleColor)
  {
    var themeWidget = new ThemeWidgetViewModel(
      this, name, sampleColor);
    ThemeWidgets.Add(themeWidget);
  }

  private void AddTheme(
    BrushConverter converter,
    string name,
    string sampleBrushColor)
  {
    var sampleColor = (SolidColorBrush?)converter.ConvertFrom(sampleBrushColor);
    if(sampleColor == null)
    {
      Trace.TraceError(
        $"Failed to convert '{sampleBrushColor}' to a SolidColorBrush");
      return;
    }
    AddTheme(name, sampleColor);
  }

  private void InitializeThemes()
  {
    var converter = new BrushConverter();
    AddTheme(converter, "Olive", "#FF6D8764");
    AddTheme(converter, "Green", "#FF60A917");
    AddTheme(converter, "Emerald", "#FF008A00");
    AddTheme(converter, "Lime", "#FFA4C400");
    AddTheme(converter, "Taupe", "#FF87794E");
    AddTheme(converter, "Yellow", "#FFFEDE06");
    AddTheme(converter, "Amber", "#FFF0A30A");
    AddTheme(converter, "Brown", "#FF825A2C");
    AddTheme(converter, "Sienna", "#FFA0522D");
    AddTheme(converter, "Orange", "#FFFA6800");
    AddTheme(converter, "Crimson", "#FFA20025");
    AddTheme(converter, "Red", "#FFE51400");
    AddTheme(converter, "Magenta", "#FFD80073");
    AddTheme(converter, "Pink", "#FFF472D0");
    AddTheme(converter, "Violet", "#FFAA00FF");
    AddTheme(converter, "Mauve", "#FF76608A");
    AddTheme(converter, "Purple", "#FF6459DF");
    AddTheme(converter, "Indigo", "#FF6A00FF");
    AddTheme(converter, "Cobalt", "#FF0050EF");
    AddTheme(converter, "Blue", "#FF0078D7");
    AddTheme(converter, "Cyan", "#FF1BA1E2");
    AddTheme(converter, "Steel", "#FF647687");
    AddTheme(converter, "Teal", "#FF00ABA9");
  }

}
