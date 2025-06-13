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
using System.Windows;
using System.Windows.Data;

using LcLauncher.IconUpdates;
using LcLauncher.Main.Rack;
using LcLauncher.Main.Rack.Tile;
using LcLauncher.Persistence;
using LcLauncher.ShellApps;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.AppPicker;

public class AppSelectorViewModel: EditorViewModelBase, IPersisted
{
  public AppSelectorViewModel(
    RackViewModel rack,
    TileHostViewModel target)
    : base(rack.Owner, "Application Selector", target.Shelf.Theme)
  {
    Wide = true;
    IconTargetId = Guid.NewGuid();
    Target = target;
    AppCache = rack.Owner.AppCache;
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
    IconJobQueue = new IconListQueue(rack.IconLoadQueue, this, IconTargetId);
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

  public AppViewModel? SelectedApp {
    get => _selectedApp;
    set {
      if(SetNullableInstanceProperty(ref _selectedApp, value))
      {
        SelectionTileKind = _selectedApp?.Category.DefaultTileKind;
        RaisePropertyChanged(nameof(SupportsExeTile));
        RaisePropertyChanged(nameof(SupportsDocTile));
        RaisePropertyChanged(nameof(SupportsUriTile));
        RaisePropertyChanged(nameof(SupportsAppTile));
      }
    }
  }
  private AppViewModel? _selectedApp;

  public TileKind? SelectionTileKind {
    get => _tileKind;
    set {
      if(SetValueProperty(ref _tileKind, value))
      {
      }
    }
  }
  private TileKind? _tileKind;

  public bool SupportsExeTile { get => SelectedApp != null && SelectedApp.SupportsRawTile; }

  public bool SupportsDocTile { get => SelectedApp != null && SelectedApp.SupportsDocTile; }

  public bool SupportsUriTile { get => SelectedApp != null && SelectedApp.SupportsUriTile; }

  public bool SupportsAppTile { get => SelectedApp != null && SelectedApp.SupportsAppTile; }

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
    return
      String.IsNullOrEmpty(FilterText)
      || appViewModel.Label
         .Contains(FilterText, StringComparison.OrdinalIgnoreCase)
      || appViewModel.Descriptor.ParsingName
         .Contains(FilterText, StringComparison.OrdinalIgnoreCase)
      || (appViewModel.Descriptor.FileSystemPath != null &&
          appViewModel.Descriptor.FileSystemPath
          .Contains(FilterText, StringComparison.OrdinalIgnoreCase));
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
      var app = new AppViewModel(this, descriptor);
      Applications.Add(app);
    }
    RecountAll();
  }

  public override bool CanAcceptEditor()
  {
    if(SelectedApp == null || SelectionTileKind == null)
    {
      return false;
    }
    if(!SelectedApp.SupportsTile(SelectionTileKind.Value))
    {
      return false;
    }
    return true;
  }

  public override void AcceptEditor()
  {
    if(CanAcceptEditor())
    {
      var app = SelectedApp!;
      var tileKind = SelectionTileKind!.Value;
      var editor2 = LaunchEditViewModel.CreateFromAppSelector(Target, app, tileKind);
      if(editor2 != null)
      {
        IsActive = false;
        editor2.IsActive = true;
      }
      else
      {
        Trace.TraceError(
          "Failed to create tile specific editor");
        IsActive = false;
      }
    }
  }

  /// <summary>
  /// This selector view model ID in the icon load system
  /// </summary>
  public Guid IconTargetId { get; }

  public IconListQueue IconJobQueue { get; }

  /// <summary>
  /// Dummy implementation (because <see cref="SaveIfDirty"/> is a dummy implementation)
  /// </summary>
  public bool IsDirty { get; private set; }

  /// <summary>
  /// Dummy implementation (because <see cref="SaveIfDirty"/> is a dummy implementation)
  /// </summary>
  public void MarkDirty()
  {
    IsDirty = true;
  }

  /// <summary>
  /// Dummy implementation (doesn't do anything beyond marking as not dirty)
  /// </summary>
  public void SaveIfDirty()
  {
    if(IsDirty)
    {
      IsDirty = false;
    }
  }
}
