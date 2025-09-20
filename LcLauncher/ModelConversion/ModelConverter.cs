/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ttelcl.Persistence.API;

using LcLauncher.Main;
using LcLauncher.Main.Rack;
using LcLauncher.DataModel.Store;

using Model2 = LcLauncher.Models;
using Model3 = LcLauncher.DataModel.Entities;

namespace LcLauncher.ModelConversion;

/// <summary>
/// Temporary model format conversion code
/// </summary>
public class ModelConverter
{
  public ModelConverter(
    MainViewModel host)
  {
    Host = host;
  }

  public MainViewModel Host { get; }

  public void ConvertCurrentRack()
  {
    var idMappings = new IdConversionMap();
    var sourceRackVm = Host.CurrentRack;
    if(sourceRackVm == null)
    {
      Trace.TraceError("There is no current rack!");
      return;
    }
    Model2::RackData sourceRack = sourceRackVm.Model.RackData;
    var sourceRackName = sourceRackVm.Model.RackName;
    var provider = Host.HyperStore.Backing.DefaultProvider;
    var targetKey =
      new StoreKey(provider.ProviderName, "rack", sourceRackName);
    var targetRackStore = Host.HyperStore.GetRackStore(targetKey);
    Trace.TraceInformation($"Erasing store {targetKey}");
    targetRackStore.Erase();

    // Phase 1: convert the rack record itself
    var targetRack = ConvertRackRecord(
      sourceRackVm,
      targetRackStore);

    // Phase 2: convert the shelf records (but not the tiles)
    ConvertShelves(
      idMappings,
      sourceRackVm,
      targetRackStore);
  }

  private static Model3.RackData ConvertRackRecord(
    RackViewModel sourceRackVm,
    LauncherRackStore targetRackStore)
  {
    var sourceRackName = sourceRackVm.Model.RackName;
    Model3::RackData? targetRack = targetRackStore.FindRack();
    if(targetRack != null)
    {
      // If this would happen something has changed in code that
      // invalidates many assumptions made further down. Jump out early.
      throw new InvalidOperationException(
        "Internal error: Expecting rack store to be empty and not have a rack record after Erasing");
    }
    var targetRackId = TickId.New();
    // There is no Guid to associate the new TickId with
    // Nor are there column Guids
    var targetColumns = new List<Model3.ColumnData>();
    foreach(var rackColumnVm in sourceRackVm.Columns)
    {
      var targetColumn =
        new Model3.ColumnData(
          TickId.New(), // there was no equivalent Guid before
          rackColumnVm.Shelves.Select(svm => svm.Model.Shelf.Tid),
          null); // auto-generate column name
      targetColumns.Add(targetColumn);
    }
    targetRack = new Model3.RackData(
      targetRackId,
      sourceRackName,
      targetColumns);
    targetRackStore.PutRack(targetRack);
    
    return targetRack;
  }

  private static void ConvertShelves(
    IdConversionMap idMappings,
    RackViewModel sourceRackVm,
    LauncherRackStore targetRackStore)
  {
    foreach(var shelf in sourceRackVm.AllShelves())
    {
      var sourceShelf = shelf.Model.Shelf;
      // Record all shelf ID mappings, since we will need them for
      // tile list mappings later on.
      idMappings[sourceShelf.IdOld] = sourceShelf.Tid;
      var targetShelf = new Model3.ShelfData(
        sourceShelf.Title,
        sourceShelf.Collapsed,
        sourceShelf.Theme,
        sourceShelf.Tid);
      targetRackStore.PutShelf(targetShelf);
    }
  }

}
