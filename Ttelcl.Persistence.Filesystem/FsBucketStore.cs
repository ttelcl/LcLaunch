/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Ttelcl.Persistence.API;

namespace Ttelcl.Persistence.Filesystem;

/// <summary>
/// Implements <see cref="IBucketStore"/> and its specializations using
/// files and folders directly
/// </summary>
public class FsBucketStore:
  IBucketStore, IJsonBucketStore, IBlobBucketStore, IHasFolder, ISingletonStore
{
  private readonly Dictionary<string, IBucketBase> _bucketRegistry;

  /// <summary>
  /// Create a new FsBucketStore
  /// </summary>
  public FsBucketStore(
    string storeFolder)
  {
    _bucketRegistry = new Dictionary<string, IBucketBase>(
      StringComparer.OrdinalIgnoreCase);
    StoreFolder = Path.GetFullPath(storeFolder);
    if(!Directory.Exists(StoreFolder))
    {
      Directory.CreateDirectory(StoreFolder);
    }
  }

  /// <summary>
  /// The folder where the store lives
  /// </summary>
  public string StoreFolder { get; }

  /// <inheritdoc/>
  public string StorageFolder => StoreFolder;

  /// <inheritdoc/>
  public IBucketBase? GetBucket(string bucketName)
  {
    if(!NamingRules.IsValidBucketName(bucketName))
    {
      throw new ArgumentOutOfRangeException(
        nameof(bucketName), 
        $"Not a valid bucket name: '{bucketName}'");
    }
    return _bucketRegistry.TryGetValue(bucketName, out var baseBucket)
      ? baseBucket
      : null;
  }

  /// <inheritdoc/>
  public IJsonBucket<T>? GetJsonBucket<T>(
    string bucketName, bool create = false)
    where T : class
  {
    if(!NamingRules.IsValidBucketName(bucketName))
    {
      throw new ArgumentException(
        $"Not a valid bucket name: '{bucketName}'");
    }
    var bucket = this.GetBucket<T>(bucketName);
    if(bucket == null)
    {
      if(create)
      {
        var b = new FsJsonBucket<T>(this, bucketName);
        _bucketRegistry.Add(bucketName, b);
        return b;
      }
      return null;
    }
    if(bucket is not IJsonBucket<T> typedBucket)
    {
      throw new InvalidOperationException(
        $"Unexpected implementation type for bucket '{bucketName}' (not a JSON bucket)");
    }
    return typedBucket;
  }

  /// <inheritdoc/>
  public IBlobBucket? GetBlobBucket(
    string bucketName, bool create = false)
  {
    if(!NamingRules.IsValidBucketName(bucketName))
    {
      throw new ArgumentException(
        $"Not a valid bucket name: '{bucketName}'");
    }
    var bucket = this.GetBucket<byte[]>(bucketName);
    if(bucket == null)
    {
      if(create)
      {
        var b = new FsBlobBucket(this, bucketName);
        _bucketRegistry.Add(bucketName, b);
        return b;
      }
      return null;
    }
    if(bucket is not IBlobBucket typedBucket)
    {
      throw new InvalidOperationException(
        $"Unexpected implementation type for bucket '{bucketName}' (not a BLOB bucket)");
    }
    return typedBucket;
  }

  /// <inheritdoc/>
  public bool TryGetSingleton<T>(
    string? typename,
    [NotNullWhen(true)] out T? value,
    string key = "singleton")
    where T : class
  {
    typename ??= SingletonStore.DefaultTypeName<T>();
    var binary = typeof(T) == typeof(byte[]);
    var shortName = GetShortSingletonFilename(typename, binary, key);
    var fileName = Path.Combine(StoreFolder, shortName);
    if(File.Exists(fileName))
    {
      if(binary)
      {
        var blob = File.ReadAllBytes(fileName);
        value = (T)(object)blob;
      }
      else
      {
        var json = File.ReadAllText(fileName);
        value = JsonConvert.DeserializeObject<T>(json)!;
      }
      return true;
    }
    else
    {
      value = null;
      return false;
    }
  }

  /// <inheritdoc/>
  public void PutSingleton<T>(
    string? typename,
    T? value,
    string key = "singleton")
    where T : class
  {
    typename ??= SingletonStore.DefaultTypeName<T>();
    var binary = typeof(T) == typeof(byte[]);
    var shortName = GetShortSingletonFilename(typename, binary, key);
    var fileName = Path.Combine(StoreFolder, shortName);
    if(value == null)
    {
      if(File.Exists(fileName))
      {
        var bak = fileName + ".bak";
        File.Move(fileName, bak, true);
      }
    }
    else
    {
      var tmp = fileName + ".tmp";
      if(binary)
      {
        var blob = (byte[])(object)value;
        File.WriteAllBytes(tmp, blob);
      }
      else
      {
        var json = JsonConvert.SerializeObject(value, Formatting.Indented);
        File.WriteAllText(tmp, json);
      }
      if(File.Exists(fileName))
      {
        var bak = fileName + ".bak";
        if(File.Exists(bak))
        {
          File.Delete(bak);
        }
        File.Replace(tmp, fileName, bak);
      }
      else
      {
        File.Move(tmp, fileName);
      }
    }
  }

  /// <inheritdoc/>
  public void EraseSingletons()
  {
    var fileNames = new List<string>();
    fileNames.AddRange(
      Directory.GetFiles(StoreFolder, "singleton.*-json"));
    fileNames.AddRange(
      Directory.GetFiles(StoreFolder, "singleton.*-blob"));
    foreach(var f in fileNames)
    {
      Trace.TraceWarning($"Erasing singleton {Path.GetFileName(f)}");
      var bak = f + ".bak";
      File.Move(f, bak, true);
    }
  }

  private void ValidateTypename(
    string typename)
  {
    if(!NamingRules.IsValidBucketName(typename))
    {
      throw new ArgumentException(
        $"Invalid type name '{typename}'");
    }
  }

  private void ValidateKeyName(
    string key)
  {
    if(!NamingRules.IsValidSingletonKey(key))
    {
      throw new ArgumentException(
        $"Invalid key name '{key}'");
    }
    //if(TickId.TryParse(key, out _))
    //{
    //  throw new ArgumentException(
    //    $"Invalid key name '{key}'. Key names must not be valid TickIds");
    //}
    //if(HashId.TryParse(key, out _))
    //{
    //  throw new ArgumentException(
    //    $"Invalid key name '{key}'. Key names must not be valid HashIds");
    //}
  }

  private string GetShortSingletonFilename(
    string typename,
    bool binary,
    string key)
  {
    ValidateTypename(typename);
    ValidateKeyName(key);
    var extendedKey =
      key == "singleton" ? "singleton" : $"singleton.{key}";
    return
      binary
      ? $"{extendedKey}.{typename}-blob"
      : $"{extendedKey}.{typename}-json";
  }
}