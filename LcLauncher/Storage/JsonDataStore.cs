/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LcLauncher.Storage.BlobsStorage;

namespace LcLauncher.Storage;

/// <summary>
/// Database for persisting data for the Launcher (low level,
/// not storage neutral)
/// </summary>
public class JsonDataStore
{
  /// <summary>
  /// Create a new LcLaunchDataStore and ensure the data folder exists.
  /// </summary>
  public JsonDataStore(
    string? dataFolder = null)
  {
    DataFolder = String.IsNullOrEmpty(dataFolder)
      ? DefaultFolder
      : Path.GetFullPath(dataFolder);
    if(!Directory.Exists(DataFolder))
    {
      Directory.CreateDirectory(DataFolder);
    }
  }

  public string DataFolder { get; }

  public static string DefaultFolder { get; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Lcl",
    "launcher");

  public T? LoadData<T>(
    string tag,
    string extension) where T : class
  {
    var path = GetDataFileName(tag, extension);
    if(!File.Exists(path))
    {
      return null;
    }
    return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
  }

  public T? LoadData<T>(
    Guid id,
    string extension) where T : class
  {
    return LoadData<T>(id.ToString(), extension);
  }

  public void SaveData<T>(
    string tag,
    string extension,
    T data)
  {
    var path = GetDataFileName(tag, extension);
    var json = JsonConvert.SerializeObject(data, Formatting.Indented);
    var tmpPath = path + ".tmp";
    File.WriteAllText(tmpPath, json);
    if(File.Exists(path))
    {
      var bakName = path + ".bak";
      if(File.Exists(bakName))
      {
        File.Delete(bakName);
      }
      File.Replace(tmpPath, path, bakName);
    }
    else
    {
      File.Move(tmpPath, path);
    }
  }

  public void SaveData<T>(
    Guid id,
    string extension,
    T data)
  {
    SaveData(id.ToString(), extension, data);
  }

  public string GetDataFileName(
    string tag,
    string extension)
  {
    if(tag.IndexOfAny(['/', '\\']) >= 0)
    {
      throw new ArgumentException("Tag must not contain directory separators.");
    }
    if(!extension.StartsWith('.'))
    {
      throw new ArgumentException("Extension must start with a period.");
    }
    return Path.Combine(DataFolder, tag + extension);
  }

  /// <summary>
  /// Get the blobs storage for the given tag.
  /// </summary>
  /// <param name="tag">
  /// The file name prefix
  /// </param>
  /// <param name="initialize">
  /// If true, the storage is initialized, i.e. the blobs file
  /// and index are created if they did not exist.
  /// </param>
  /// <returns></returns>
  public BlobStorage GetBlobs(
    string tag,
    bool initialize)
  {
    return new BlobStorage(GetDataFileName(tag, ".blobs"), initialize);
  }

  public IconCache GetIconCache(
    Guid cacheId)
  {
    return new IconCache(this, cacheId);
  }
}
