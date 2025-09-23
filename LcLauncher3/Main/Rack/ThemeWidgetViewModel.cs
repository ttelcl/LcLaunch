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
using System.Windows.Media;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class ThemeWidgetViewModel: ViewModelBase
{
  public ThemeWidgetViewModel(
    ThemePickerViewModel owner,
    string theme,
    Brush sampleColor)
  {
    Owner = owner;
    Theme = theme;
    SampleColor = sampleColor;
    MakeActiveCommand = new DelegateCommand(
      p => IsCurrent = true);
  }

  public ICommand MakeActiveCommand { get; }

  public ThemePickerViewModel Owner { get; }

  public string Theme { get; }

  public Brush SampleColor { get; }

  public bool IsCurrent {
    get => _isCurrent;
    set {
      if(SetValueProperty(ref _isCurrent, value))
      {
        if(value && Owner.CurrentThemeWidget != this)
        {
          Owner.CurrentThemeWidget = this;
        }
        RaisePropertyChanged(nameof(SelectedIcon));
      }
    }
  }
  private bool _isCurrent;

  public string SelectedIcon => IsCurrent ? "RadioboxMarked" : "RadioboxBlank";
}
