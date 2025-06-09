/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using LcLauncher.Main.Rack.Tile;
using LcLauncher.ShellApps;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.AppPicker;

public class AppSelectorViewModel: EditorViewModelBase
{
  public AppSelectorViewModel(
    MainViewModel owner,
    TileHostViewModel target)
    : base(owner, "Application Selector", target.Shelf.Theme)
  {
    Wide = true;
    Target = target;
    AppCache = owner.AppCache;
    Applications = [];
    ViewSource = new CollectionViewSource();
    ViewSource.Source = Applications;
    var view = ViewSource.View;
    if(view is ListCollectionView lcv)
    {
      ApplicationsView = lcv;
    }
    else
    {
      throw new InvalidOperationException(
        "Unexpected list view type");
    }
    ApplicationsView.Filter = AppCategory.NonFilter;
    Categories = [];
    foreach(var category in AppCategory.AppCategoryList)
    {
      var catVm = new AppCategoryViewModel(this, category);
      Categories.Add(catVm);
    }
    Categories.Add(new AppCategoryViewModel(this, null));
    SelectedCategory = Categories[0];
    Refill();
  }

  public TileHostViewModel Target { get; }

  public ShellAppCache AppCache { get; }

  public ObservableCollection<AppViewModel> Applications { get; }

  public ObservableCollection<AppCategoryViewModel> Categories { get; }

  public CollectionViewSource ViewSource { get; }

  public ListCollectionView ApplicationsView { get; }

  public AppCategory? CategoryFilter {
    get => _appCategory;
    private set {
      if(SetNullableInstanceProperty(ref _appCategory, value))
      {
        ApplicationsView.Filter = ViewFilter;
        RaisePropertyChanged(nameof(CategoryFilterName));
        var txt = _appCategory?.Key ?? "<none>";
        var count = ApplicationsView.Count;
        Trace.TraceInformation($"Application filter = '{txt}' ({count})");
      }
    }
  }
  private AppCategory? _appCategory;

  public string FilterText {
    get => _filterText;
    set {
      if(SetValueProperty(ref _filterText, value))
      {
        ApplicationsView.Filter = ViewFilter;
        RecountAll();
      }
    }
  }
  private string _filterText = string.Empty;

  public string? CategoryFilterName {
    get => _appCategory?.Key;
    private set {
      if(String.IsNullOrEmpty(value))
      {
        CategoryFilter = null;
      }
      else if(AppCategory.AppCategoryMap.TryGetValue(value, out var category))
      {
        CategoryFilter = category;
      }
      else
      {
        CategoryFilter = null;
      }
    }
  }

  private bool ViewFilter(object appViewModel)
  {
    if(SelectedCategory == null)
    {
      return false;
    }
    if(appViewModel is AppViewModel avm)
    {
      if(!SelectedCategory.IsMatch(avm))
      {
        return false;
      }
      return MatchFilterText(avm);
    }
    else
    {
      return false;
    }
  }

  public bool MatchFilterText(AppViewModel appViewModel)
  {
    if(String.IsNullOrEmpty(FilterText))
    {
      return true;
    }
    return
      appViewModel.Label
      .Contains(FilterText, StringComparison.OrdinalIgnoreCase);
  }

  public AppCategoryViewModel SelectedCategory { 
    get => _selectedCategory;
    set {
      var oldCategory = _selectedCategory;
      if(SetInstanceProperty(ref _selectedCategory, value))
      {
        if(oldCategory != null)
        {
          oldCategory.IsSelected = false;
        }
        CategoryFilterName = _selectedCategory.Key;
        if(_selectedCategory != null)
        {
          _selectedCategory.IsSelected = true;
        }
      }
    }
  }
  private AppCategoryViewModel _selectedCategory = null!;

  public void RecountAll()
  {
    foreach(var acvm in Categories)
    {
      acvm.Recount();
    }
  }

  public void Refill()
  {
    Refill(TimeSpan.FromMinutes(30));
  }
  
  public void Refill(TimeSpan minAge)
  {
    AppCache.Refill(minAge);
    Applications.Clear();
    var descriptors =
      from descriptor in AppCache.Descriptors
      orderby descriptor.Label
      select descriptor;
    foreach(var descriptor in descriptors)
    {
      var app = new AppViewModel(descriptor);
      Applications.Add(app);
    }
    RecountAll();
  }
}
