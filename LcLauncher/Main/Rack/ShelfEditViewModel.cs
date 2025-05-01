/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class ShelfEditViewModel: EditorViewModelBase
{
  public ShelfEditViewModel(
    ShelfViewModel target)
    : base(
      target.Rack.Owner,
      $"Shelf {target.ShelfId}",
      target.Theme)
  {
    Target = target;
    Title = target.Title;
    ThemePicker = new ThemePickerViewModel(
      Target.Theme,
      this);
  }

  public static void Show(
    ShelfViewModel target)
  {
    var instance = new ShelfEditViewModel(target);
    instance.IsActive = true;
  }

  public ShelfViewModel Target { get; }

  public string Title {
    get => _title;
    set {
      if(SetValueProperty(ref _title, value))
      {
      }
    }
  }
  private string _title = string.Empty;

  public ThemePickerViewModel ThemePicker { get; }

  public override bool CanAcceptEditor()
  {
    return
      !String.IsNullOrEmpty(Title)
      && (Title != Target.Title || Theme != Target.Theme);
  }

  public override void AcceptEditor()
  {
    if(CanAcceptEditor())
    {
      Target.Title = Title;
      Target.Theme = Theme;
      Target.MarkDirty();
      Target.SaveIfDirty();
      IsActive = false;
    }
  }
}
