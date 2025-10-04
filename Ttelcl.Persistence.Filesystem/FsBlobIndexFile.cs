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

using Ttelcl.Persistence.API;

namespace Ttelcl.Persistence.Filesystem;

/// <summary>
/// File-backed Index of a <see cref="FsBlobsFile"/>
/// </summary>
public class FsBlobIndexFile
{
  private readonly Dictionary<HashId, FsBlobDescriptor> _entryMap;

  /// <summary>
  /// Create a new FsBlobIndexFile
  /// </summary>
  public FsBlobIndexFile(
    FsBlobsFile target)
  {
    _entryMap = new Dictionary<HashId, FsBlobDescriptor>();
    Target = target;
    FileName = Path.ChangeExtension(target.FileName, ".blob-idx");
    InitFile();
    Reload();
  }

  /// <summary>
  /// The target blobs file
  /// </summary>
  public FsBlobsFile Target { get; }

  /// <summary>
  /// The path of the index file
  /// </summary>
  public string FileName { get; }

  /// <summary>
  /// Get a view on the map of blob descriptors
  /// </summary>
  public IReadOnlyDictionary<HashId, FsBlobDescriptor> Descriptors => _entryMap;

  /// <summary>
  /// Read a blob given by ID from the open blobs file, returning null if not found
  /// </summary>
  /// <param name="blobReader"></param>
  /// <param name="id"></param>
  /// <returns></returns>
  public byte[]? ReadBlob(FileStream blobReader, HashId id)
  {
    return _entryMap.TryGetValue(id, out var descriptor)
      ? Target.ReadBlob(blobReader, descriptor)
      : null;
  }

  /// <summary>
  /// If the blob identified by <paramref name="id"/> is found in this
  /// index, then open the blobs file and read the blob.
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  public byte[]? ReadBlob(HashId id)
  {
    if(_entryMap.TryGetValue(id, out var descriptor))
    {
      using var blobReader = Target.OpenRead();
      return Target.ReadBlob(blobReader, descriptor);
    }
    else
    {
      return null;
    }
  }

  internal void Reload()
  {
    using var fs = OpenRead();
    Span<byte> buffer = stackalloc byte[EntrySize];
    _entryMap.Clear();
    while(fs.Read(buffer) == buffer.Length)
    {
      var descriptor = FsBlobDescriptor.FromBytes(buffer);
      if(_entryMap.TryGetValue(descriptor.Id, out var e))
      {
        Trace.TraceError(
          $"Corruption in '{FileName}'. Duplicate entry {descriptor.Id}. Replacing earlier value");
      }
      // While this situation is bad and indicates an earlier error,
      // it is not so bad that it should prevent LcLauncher from launching
      // at all, with an obscure error (which it did)
      _entryMap[descriptor.Id] = descriptor;
    }
  }

  internal bool TryAppendBlob(
    byte[] blob,
    FileStream blobAppendStream,
    FileStream indexAppendStream,
    out IBlobEntry entry)
  {
    var id = HashId.FromBlob(blob, out byte[] hash);
    if(_entryMap.TryGetValue(id, out var e))
    {
      entry = e;
      return false;
    }
    var offset = blobAppendStream.Seek(0, SeekOrigin.End);
    var descriptor = new FsBlobDescriptor(offset, blob.Length, hash);
    entry = descriptor;
    descriptor.Write(blobAppendStream, indexAppendStream, blob);
    _entryMap.Add(id, descriptor);
    return true;
  }

  /// <summary>
  /// Open the blob index file for reading. Move the read pointer to
  /// just after the header
  /// </summary>
  /// <returns></returns>
  internal FileStream OpenRead()
  {
    var fs = File.OpenRead(FileName);
    fs.Position = HeaderSize;
    return fs;
  }

  /// <summary>
  /// Open the blob index file for appending, and move the write pointer
  /// to the end
  /// </summary>
  /// <returns></returns>
  internal FileStream OpenAppend()
  {
    var fs = new FileStream(
      FileName,
      FileMode.Append,
      FileAccess.Write);
    fs.Seek(0L, SeekOrigin.End);
    return fs;
  }

  /// <summary>
  /// Make sure the file exists and has its header written out
  /// </summary>
  private void InitFile()
  {
    if(!File.Exists(FileName))
    {
      File.WriteAllBytes(FileName, BitConverter.GetBytes(Signature));
    }
  }

  internal void Erase()
  {
    if(File.Exists(FileName))
    {
      var bakName = FileName + ".bak";
      File.Move(FileName, bakName, true);
    }
    _entryMap.Clear();
    InitFile();
  }

  internal const ulong Signature = 0x58444E49424F4C42; // BLOBINDX

  internal const int EntrySize = 32;
  internal const int HeaderSize = 8;
}
