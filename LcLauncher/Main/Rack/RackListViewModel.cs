/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using LcLauncher.Models;
using LcLauncher.WpfUtilities;

using Ttelcl.Persistence.API;

namespace LcLauncher.Main.Rack;

public class RackListViewModel: ViewModelBase
{
  public RackListViewModel(
    MainViewModel owner)
  {
    Owner = owner;
    Racks = [];
    RacksNew = [];
    RefreshCommand = new DelegateCommand(p => Refresh());
    Refresh();
    SelectedRack = NoRackText;
    _selectedRack = NoRackText;
  }

  public MainViewModel Owner { get; }

  public ObservableCollection<string> Racks { get; }

  public ObservableCollection<RackInfo> RacksNew { get; }

  public ICommand RefreshCommand { get; }

  // call back from event
  public void OnDropDownOpened()
  {
    Refresh();
  }

  public string SelectedRack {
    get => _selectedRack;
    set {
      if(!Racks.Contains(value))
      {
        value = NoRackText;
      }
      if(SetValueProperty(ref _selectedRack, value))
      {
        if(_selectedRack == NoRackText)
        {
          Owner.CurrentRack = null;
        }
        else
        {
          var store = Owner.Store;
          var rack = store.LoadRack(_selectedRack);
          if(rack == null)
          {
            Trace.TraceWarning($"Rack '{_selectedRack}' not found in store.");
            MessageBox.Show(
              $"Rack '{_selectedRack}' not found.",
              "Error",
              MessageBoxButton.OK,
              MessageBoxImage.Error);
            Owner.CurrentRack = null;
            return;
          }
          var rackModel = new RackModel(store, _selectedRack);
          var rackVm = new RackViewModel(Owner, rackModel);
          //if(rackModel.RackData.Upgrading)
          //{
          //  rackVm.MarkDirty();
          //  rackVm.SaveIfDirty();
          //}
          Owner.CurrentRack = rackVm;
        }
      }
    }
  }
  private string _selectedRack;

  const string NoRackText = "<no rack loaded>";
  static readonly RackInfo NoRackInfo = new RackInfo(NoRackText, null);

  private void Refresh() {
    if(Racks.Count == 0 || !Racks.Contains(NoRackText))
    {
      Racks.Add(NoRackText);
    }
    var store = Owner.Store;
    var racks = store.EnumRacks().ToList();
    var removedRacks = new List<string>();
    foreach(var rack in Racks)
    {
      if(!racks.Contains(rack))
      {
        if(rack == NoRackText)
        {
          continue;
        }
        removedRacks.Add(rack);
      }
    }
    foreach(var rack in removedRacks)
    {
      Racks.Remove(rack);
    }
    foreach(var rack in racks)
    {
      if(!Racks.Contains(rack))
      {
        Racks.Add(rack);
      }
    }
    if(SelectedRack != null && !Racks.Contains(SelectedRack))
    {
      SelectedRack = NoRackText;
    }
    var rackNames = String.Join(", ", Racks);
    Trace.TraceInformation(
      $"Available racks: {rackNames}");
    RefreshNew();
  }

  private void RefreshNew()
  {
    if(RacksNew.Count == 0 || !RacksNew.Contains(NoRackInfo))
    {
      RacksNew.Add(NoRackInfo);
    }
    var store = Owner.HyperStore;
    var rackKeys = store.FindRackStores().Select(k => RackInfo.FromKey(k)).ToList();
    var removedRacks = new List<RackInfo>();
    foreach(var rack in RacksNew)
    {
      if(!rackKeys.Contains(rack))
      {
        if(rack.RackKey == null)
        {
          continue;
        }
        removedRacks.Add(rack);
      }
    }
    foreach(var rack in removedRacks)
    {
      RacksNew.Remove(rack);
    }
    foreach(var rack in rackKeys)
    {
      if(!RacksNew.Contains(rack))
      {
        RacksNew.Add(rack);
      }
    }
    //if(SelectedRack != null && !RacksNew.Contains(SelectedRack))
    //{
    //  SelectedRack = NoRackText;
    //}
    var rackNames = String.Join(", ", RacksNew.Select(r => r.Name));
    Trace.TraceInformation(
      $"Available NEW racks: {rackNames}");

  }

  public string? FindRackByFile(string file)
  {
    if(!file.EndsWith(".rack-json"))
    {
      // both V2 and V3 rack files end in '.rack-json'
      return null;
    }
    file = Path.GetFullPath(file);
    var shortName = Path.GetFileName(file);
    var rackTag = Path.GetFileNameWithoutExtension(file);
    if(TickId.TryParse(rackTag, out var rackId))
    {
      var pathSegments = file.Split(
        Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      var folderName = pathSegments[^2];
      var m = Regex.Match(
        folderName,
        "^[a-z0-9]{1,5}--rack--([-_0-9a-z]+)$",
        RegexOptions.IgnoreCase);
      if(m.Success)
      {
        var rackName = m.Groups[1].Value;
        return rackName;
      }
    }
    // if recognition as V3 fails, try V2
    return FindRackByPseudoFile(shortName);
  }

  public string? FindRackByPseudoFile(string pseudoFile)
  {
    if(String.IsNullOrEmpty(pseudoFile))
    {
      return null;
    }
    if(!pseudoFile.EndsWith(".rack-json"))
    {
      // both V2 and V3 rack files end in '.rack-json'
      return null;
    }
    if(pseudoFile.IndexOfAny(['/', '\\', ':', '*', '?']) >= 0)
    {
      return null;
    }
    var rackName = pseudoFile.Substring(0, pseudoFile.Length - ".rack-json".Length);
    return FindRackByName(rackName);
  }

  public string? FindRackByName(string rackName)
  {
    var rackinfo =
      RacksNew.Where(ri => ri.RackKey?.StoreName == rackName).FirstOrDefault();
    if(rackinfo != null)
    {
      return rackinfo.RackKey!.StoreName;
    }
    if(Racks.Contains(rackName))
    {
      // V2
      Trace.TraceWarning($"Found rack '{rackName}' as legacy rack, not as converted rack");
      return rackName;
    }
    return null;
  }
}

public record RackInfo(string Name, StoreKey? RackKey)
{
  public static RackInfo FromKey(StoreKey key)
  {
    return new(key.StoreName, key);
  }
}
