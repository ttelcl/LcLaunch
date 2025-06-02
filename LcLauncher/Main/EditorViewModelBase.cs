/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using ControlzEx.Theming;

using LcLauncher.Main.Rack;
using LcLauncher.Models;
using LcLauncher.WpfUtilities;

namespace LcLauncher.Main;

/// <summary>
/// Common base class for all Editors.
/// </summary>
public class EditorViewModelBase: ViewModelBase, IHasTheme
{
  /// <summary>
  /// Create a new EditorViewModelBase
  /// </summary>
  public EditorViewModelBase(
    MainViewModel owner,
    string editorTitle,
    string? theme /*= owner.DefaultTheme*/)
  {
    Owner = owner;
    EditorTitle = editorTitle;
    Theme = theme ?? Owner.DefaultTheme;
    CancelCommand = new DelegateCommand(
      p => CancelEditor());
    AcceptCommand = new DelegateCommand(
      p => AcceptEditor(),
      p => CanAcceptEditor());
  }

  public static void ShowTest(
    MainViewModel owner)
  {
    var instance = new EditorViewModelBase(
      owner, "Test", owner.DefaultTheme);
    instance.IsActive = true;
  }

  public ICommand CancelCommand { get; }

  public ICommand AcceptCommand { get; }

  public MainViewModel Owner { get; }

  public string EditorTitle { get; }

  public string Theme {
    get => _theme;
    set {
      if(SetValueProperty(ref _theme, value))
      {
        SetTheme("Dark." + _theme);
      }
    }
  }
  private string _theme = MainViewModel.DefaultDefaultTheme;

  /// <summary>
  /// Indicates if this editor is the current editor.
  /// Setting this manipulates the Owner.CurrentEditor property.
  /// </summary>
  public bool IsActive {
    get => Owner.CurrentEditor == this;
    set {
      if(value)
      {
        if(Owner.CurrentEditor != this)
        {
          Owner.CurrentEditor = this;
        }
      }
      else
      {
        if(Owner.CurrentEditor == this)
        {
          Owner.CurrentEditor = null;
        }
      }
      RaisePropertyChanged();
    }
  }

  private EditorBaseView? Host { get; set; }

  internal void UpdateHost(EditorBaseView? host)
  {
    if(host == null)
    {
      Trace.TraceInformation(
        "EditorBaseView.UpdateHost: Clearing host control");
      Host = null;
    }
    else
    {
      Trace.TraceInformation(
        "EditorBaseView.UpdateHost: Setting host control");
      Host = host;
      SetTheme("Dark." + Theme);
    }
  }

  private void SetTheme(string? rawTheme)
  {
    if(Host == null)
    {
      //Trace.TraceWarning("SetTheme: Host control is null");
      return;
    }
    if(string.IsNullOrEmpty(rawTheme))
    {
      Trace.TraceWarning("SetTheme: Theme is null or empty");
      return;
    }
    ThemeManager.Current.ChangeTheme(Host, rawTheme);
  }

  /// <summary>
  /// Close this editor without saving any changes
  /// (i.e. 'Cancel' button functionality)
  /// </summary>
  public virtual void CancelEditor()
  {
    Trace.TraceInformation(
      "EditorBaseView.CancelEditor: Cancelling editor");
    IsActive = false;
  }

  public virtual bool CanAcceptEditor()
  {
    // The default is to always disable "OK".
    return false;
  }

  /// <summary>
  /// Accept the changes made in this editor and close it.
  /// (i.e. 'OK' button functionality)
  /// </summary>
  public virtual void AcceptEditor()
  {
    if(!CanAcceptEditor())
    {
      MessageBox.Show(
        "This dialog's information is not valid",
        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      return;
    }
    // do nothing.
    IsActive = false;
  }
}
