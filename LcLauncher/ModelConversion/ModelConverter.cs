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

using LcLauncher.DataModel.Store;
using LcLauncher.Main;
using LcLauncher.Main.Rack;
using LcLauncher.Main.Rack.Tile;
using LcLauncher.Persistence;

using Model2 = LcLauncher.Models;
using Model3 = LcLauncher.DataModel.Entities;
using System.Windows.Input;
using System.IO;

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

  public bool DumpIcons { get; private set; }

  public void ConvertCurrentRack(
    bool dumpIcons = false)
  {
    var oldDumpIcons = DumpIcons;
    DumpIcons = dumpIcons;
    try
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
      Trace.TraceInformation(
        $"Starting export of rack '{sourceRackName}' as store '{targetKey}'");
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

      // Phase 3: convert tile lists and icons
      ConvertTileLists(
        idMappings,
        sourceRackVm,
        targetRackStore);

      Trace.TraceInformation(
        $"Completed export of rack '{sourceRackName}' as store '{targetKey}'");
    }
    finally
    {
      DumpIcons = oldDumpIcons;
    }
  }

  private Model3.RackData ConvertRackRecord(
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
    Trace.TraceInformation($"Converting rack {sourceRackName} to {targetRackId}");
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
      Trace.TraceInformation($"  Column: {targetColumn.Id}");
    }
    targetRack = new Model3.RackData(
      targetRackId,
      sourceRackName,
      targetColumns);
    targetRackStore.PutRack(targetRack);

    return targetRack;
  }

  private void ConvertShelves(
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
      Trace.TraceInformation(
        $"Shelf {sourceShelf.IdOld} -> {targetShelf.Id}");
      targetRackStore.PutShelf(targetShelf);
    }
  }

  private void ConvertTileLists(
    IdConversionMap idMappings,
    RackViewModel sourceRackVm,
    LauncherRackStore targetRackStore)
  {
    var sourceTileLists = sourceRackVm.GatherTileLists();
    foreach(var sourceTileList in sourceTileLists)
    {
      ConvertTileList(
        idMappings,
        sourceTileList,
        targetRackStore);
    }
  }

  private void ConvertTileList(
    IdConversionMap idMappings,
    TileListViewModel sourceList,
    LauncherRackStore targetRackStore)
  {
    var sourceModel = sourceList.Model;
    var sourceId = sourceModel.Id;
    // targetId may or may not exist already! The primary tile list
    // of a shelf shares their id with that shelf.
    var targetId = idMappings[sourceId];
    Trace.TraceInformation(
      $"Tile list {sourceId} -> {targetId}");
    var sourceTileVms =
      sourceList.Tiles
      .Select(thvm => thvm.Tile)
      .ToList();
    using var iconWriter = targetRackStore.IconBucket.OpenWriter();
    var targetTiles =
      sourceTileVms
      .Select(tvm => ConvertTile(
        idMappings,
        tvm,
        iconWriter))
      .ToList();
    var targetList =
      new Model3.TileListData(
        targetId,
        targetTiles);
    targetRackStore.PutTiles(targetList);
  }

  private Model3.TileData? ConvertTile(
    IdConversionMap idMappings,
    TileViewModel? sourceTile,
    IBlobBucketWriter iconWriter)
  {
    if(sourceTile == null)
    {
      return null;
    }
    var iconCacheSource = sourceTile.OwnerList.IconCache;
    Model3.TileData targetTile =
      sourceTile switch {
        EmptyTileViewModel etvm =>
          ConvertTile(etvm),
        GroupTileViewModel gtvm =>
          ConvertTile(gtvm, idMappings),
        LaunchTileViewModel ltvm =>
          ConvertTile(ltvm, iconCacheSource, iconWriter),
        QuadTileViewModel qtvm =>
          ConvertTile(qtvm, iconCacheSource, iconWriter),
        _ =>
          throw new InvalidOperationException(
            $"Unrecognized tile type {sourceTile.GetType().FullName}"),
      };

    return targetTile;
  }

  private Model3.TileData ConvertTile(
    EmptyTileViewModel etvm)
  {
    return Model3.TileData.EmptyTile();
  }

  private Model3.TileData ConvertTile(
    GroupTileViewModel gtvm,
    IdConversionMap idMappings)
  {
    var sourceModel = gtvm.Model;
    var targetId = idMappings[gtvm.Model.TileList];
    return Model3.TileData.GroupTile(
      new Model3.GroupData(
        targetId,
        sourceModel.Title,
        sourceModel.Tooltip));
  }

  private Model3.TileData ConvertTile(
    LaunchTileViewModel ltvm,
    ILauncherIconCache iconCache,
    IBlobBucketWriter iconWriter)
  {
    var sourceModel = ltvm.NewModel;
    // in unexpected upgrade paths, sourceModel could be null
    if(sourceModel == null)
    {
      Trace.TraceWarning(
        "Launch tile turns out to be null; changing into empty tile." +
        $" List {ltvm.OwnerList.TileListId}");
      return Model3.TileData.EmptyTile();
    }
    else
    {
      return Model3.TileData.LaunchTile(
        ConvertLaunch(sourceModel, iconCache, iconWriter));
    }
  }

  private Model3.LaunchData ConvertLaunch(
    Model2.LaunchData sourceLaunch,
    ILauncherIconCache iconCache,
    IBlobBucketWriter iconWriter)
  {
    var hash48 = ConvertIcon(sourceLaunch.Icon48, iconCache, iconWriter);
    var hash32 = ConvertIcon(sourceLaunch.Icon32, iconCache, iconWriter);
    var hash16 = ConvertIcon(sourceLaunch.Icon16, iconCache, iconWriter);
    var pathEnv = new Dictionary<string, Model3.PathEdit>();
    foreach(var kvp2 in sourceLaunch.PathEnvironment)
    {
      var key = kvp2.Key;
      var value = new Model3.PathEdit(
        kvp2.Value.Prepend,
        kvp2.Value.Append);
      pathEnv.Add(key, value);
    }
    var targetLaunchData = new Model3.LaunchData(
      sourceLaunch.Target,
      sourceLaunch.ShellMode,
      sourceLaunch.Title,
      sourceLaunch.Tooltip,
      sourceLaunch.WindowStyle,
      sourceLaunch.IconSource,
      hash48,
      hash32,
      hash16,
      sourceLaunch.Verb,
      sourceLaunch.WorkingDirectory,
      sourceLaunch.Arguments,
      sourceLaunch.Environment,
      pathEnv);
    return targetLaunchData;
  }

  private HashId? ConvertIcon(
    string? iconKey,
    ILauncherIconCache iconCache,
    IBlobBucketWriter iconWriter)
  {
    if(String.IsNullOrEmpty(iconKey))
    {
      return null;
    }
    var blob = iconCache.LoadCachedBlob(iconKey);
    if(blob == null)
    {
      return null;
    }
    var stored = iconWriter.TryPutBlob(blob, out var id);
    if(DumpIcons
      && stored
      && iconWriter is IHasFolder hasFolder)
    {
      var folder = hasFolder.StorageFolder;
      var fnm = Path.Combine(folder, $"icon.{id}.png");
      File.WriteAllBytes(fnm, blob);
    }
    return id;
  }

  private Model3.TileData ConvertTile(
    QuadTileViewModel qtvm,
    ILauncherIconCache iconCache,
    IBlobBucketWriter iconWriter)
  {
    return Model3.TileData.QuadTile(
      qtvm.RawModel.Select(
        ld => ld == null ? null : ConvertLaunch(ld, iconCache, iconWriter)));
  }

}
