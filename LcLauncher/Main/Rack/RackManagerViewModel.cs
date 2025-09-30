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
using System.Windows;
using System.Windows.Input;

using Ttelcl.Persistence.API;

using LcLauncher.WpfUtilities;

namespace LcLauncher.Main.Rack;

public class RackManagerViewModel: ViewModelBase
{
  public RackManagerViewModel(
    MainViewModel main)
  {
    Main = main;
    Providers = [.. Main.HyperStore.Backing.ProviderNames];
    _selectedProvider = Main.HyperStore.Backing.DefaultProviderName;
    ShowSelectedRackCommand = new DelegateCommand(
      p => { RackList.Selected = Selection ?? RackList.NoRack; },
      p => Selection != null);
    CreateNewRackCommand = new DelegateCommand(
      p => CreateNewRack(),
      p => NewNameIsValid);
  }

  public ICommand ShowSelectedRackCommand { get; }

  public ICommand CreateNewRackCommand { get; }

  public MainViewModel Main { get; }

  public RackListViewModel RackList => Main.RackList;

  public ObservableCollection<string> Providers { get; }

  public string SelectedProvider {
    get => _selectedProvider;
    set {
      if(SetValueProperty(ref _selectedProvider, value))
      {
      }
    }
  }
  private string _selectedProvider;

  public string NewRackName {
    get => _newRackName;
    set {
      if(SetValueProperty(ref _newRackName, value))
      {
        CheckNewRackName();
      }
    }
  }
  private string _newRackName = "";

  public bool NewNameIsValid {
    get => _newRackNameIsValid;
    private set {
      if(SetValueProperty(ref _newRackNameIsValid, value))
      {
      }
    }
  }
  private bool _newRackNameIsValid = false;

  public string NewRackNameError {
    get => _newRackNameError;
    private set {
      if(SetValueProperty(ref _newRackNameError, value))
      {
      }
    }
  }
  private string _newRackNameError = "";

  public void ClearNewRackName()
  {
    NewRackName = "";
  }

  private void CheckNewRackName()
  {
    if(String.IsNullOrEmpty(NewRackName))
    {
      // Not valid, but don't shout out what's wrong
      NewRackNameError = String.Empty;
      NewNameIsValid = false;
      return;
    }
    if(!NamingRules.IsValidStoreName(NewRackName))
    {
      NewRackNameError =
        "Not a valid rack name (only use letters and digits, optionally separated by '-').";
      NewNameIsValid = false;
      return;
    }
    if(Main.HyperStore.FindRacksByName(NewRackName).Any())
    {
      NewRackNameError =
        "That name is already in use.";
      NewNameIsValid = false;
      return;
    }
    NewRackNameError = String.Empty;
    NewNameIsValid = true;
  }

  public RackReferenceViewModel? Selection {
    get => _selection;
    set {
      if(value != null && !value.IsValid)
      {
        value = null;
      }
      if(SetNullableInstanceProperty(ref _selection, value))
      {
      }
    }
  }
  private RackReferenceViewModel? _selection;

  private void CreateNewRack()
  {
    CheckNewRackName();
    if(NewNameIsValid) // else silently do nothing
    {
      var name = NewRackName;
      var provider = SelectedProvider;
      if(!Main.HyperStore.CreateRackIfNotExists(
        name, provider))
      {
        Trace.TraceError(
          $"Error creating rack '{name}' using provider '{provider}'");
      }
      else
      {
        Trace.TraceInformation(
          $"Successfully created rack '{name}' using provider '{provider}'");
      }
      RackList.Refresh();
      ClearNewRackName();
      var vms =
        RackList.Racks.
        Where(r =>
            r.RackName.Equals(name, StringComparison.OrdinalIgnoreCase)
            && r.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase))
        .ToList();
      if(vms.Count == 1) // should always be the case
      {
        var vm = vms[0];
        Selection = vm;
      }
      else
      {
        Trace.TraceError(
          $"Trying to find the just created rack found {vms.Count} racks instead of one");
      }
    }
  }
}
