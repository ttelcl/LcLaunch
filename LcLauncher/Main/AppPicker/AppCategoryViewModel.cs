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

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.AppPicker;

public class AppCategoryViewModel: ViewModelBase
{
  public AppCategoryViewModel(
    AppSelectorViewModel owner,
    AppCategory? category)
  {
    Owner = owner;
    Category = category;
    Label = category?.Key ?? "All";
    Recount();
  }

  public bool IsSelected {
    get => _isSelected;
    set {
      if(SetValueProperty(ref _isSelected, value))
      {
        if(value)
        {
          Owner.SelectedCategory = this;
        }
      }
    }
  }
  private bool _isSelected;

  public int FilterCount {
    get => _filterCount;
    set {
      if(SetValueProperty(ref _filterCount, value))
      {
        RaisePropertyChanged(nameof(FilterCountText));
      }
    }
  }
  private int _filterCount = 0;

  public string FilterCountText {
    get => $"({FilterCount})";
  }

  public AppSelectorViewModel Owner { get; }

  public AppCategory? Category { get; }

  public bool IsMatch(AppViewModel avm)
  {
    return Category == null || Category == avm.Category;
  }

  public void Recount()
  {
    FilterCount =
      Owner.Applications.Count(
        avm => IsMatch(avm) && Owner.MatchFilterText(avm));
  }

  public string? Key { get => Category?.Key; }

  public string Label { get; }
}
