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

using Ttelcl.Persistence.API;

namespace Ttelcl.Persistence.Filesystem;

/// <summary>
/// Description of FsJsonBucket
/// </summary>
public class FsJsonBucket<T>: IJsonBucket<T> where T : class
{
  /// <summary>
  /// Create a new FsJsonBucket
  /// </summary>
  internal FsJsonBucket(
    FsBucketStore store,
    string bucketName)
  {
    Store = store;
    BucketName = bucketName;
  }

  /// <summary>
  /// The store this bucket is part of
  /// </summary>
  public FsBucketStore Store { get; }

  /// <summary>
  /// The name of the bucket
  /// </summary>
  public string BucketName { get; }

  /// <summary>
  /// The type of items stored in this bucket (<typeparamref name="T"/>)
  /// </summary>
  public Type BucketType => typeof(T);

  /// <inheritdoc/>
  public bool ContainsKey(TickId key)
  {
    var fileName = ItemFileName(key);
    return File.Exists(fileName);
  }

  /// <inheritdoc/>
  public T? this[TickId key] {
    get {
      var fileName = ItemFileName(key);
      if(File.Exists(fileName))
      {
        var json = File.ReadAllText(fileName);
        var settings = new JsonSerializerSettings {
          DateParseHandling = DateParseHandling.None,
        };
        var t =
          JsonConvert.DeserializeObject<T>(json, settings)
          ?? throw new InvalidOperationException(
            "Internal error - not expecting deserialization to null");
        return t;
      }
      else
      {
        return null;
      }
    }
    set {
      var fileName = ItemFileName(key);
      if(value == null)
      {
        if(File.Exists(fileName))
        {
          var bakName = fileName + ".bak";
          File.Move(fileName, bakName, true);
        }
      }
      else
      {
        var tmpName = fileName + ".tmp";
        var json = JsonConvert.SerializeObject(value, Formatting.Indented);
        File.WriteAllText(tmpName, json);
        if(File.Exists(fileName))
        {
          var bakName = fileName + ".bak";
          if(File.Exists(bakName))
          {
            File.Delete(bakName);
          }
          File.Replace(tmpName, fileName, bakName);
        }
        else
        {
          File.Move(tmpName, fileName);
        }
      }
    }
  }

  /// <inheritdoc/>
  public IEnumerable<TickId> Keys()
  {
    var di = new DirectoryInfo(Store.StoreFolder);
    var mask = $"????????-??????-???????.{BucketName}.json";
    foreach(var file in di.GetFiles(mask))
    {
      var idtag = file.Name.Substring(0, 23);
      if(TickId.TryParse(idtag, out var id))
      {
        yield return id;
      }
    }
  }

  private string ItemFileName(TickId id)
  {
    var shortName = $"{id}.{BucketName}.json";
    var fullName = Path.Combine(Store.StoreFolder, shortName);
    return fullName;
  }
}
