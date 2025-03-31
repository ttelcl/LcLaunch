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

namespace LcLauncher.Storage.BlobsStorage;

public class BlobIndexFile
{
  private List<BlobEntry> _entries;

  internal BlobIndexFile(
    BlobStorage owner)
  {
    _entries = [];
    Entries = _entries.AsReadOnly();
    Owner = owner;
  }

  public BlobStorage Owner { get; }

  public string FileName => Owner.IndexFileName;

  public IReadOnlyList<BlobEntry> Entries { get; }

  /// <summary>
  /// Update the index based on the current state of the blobs file.
  /// If the blobs file does not exist, the index will be deleted.
  /// Otherwise, the index will be updated to contain any new blobs
  /// appended to the blobs file since the last update. If necessary,
  /// the index file will be created.
  /// </summary>
  /// <returns>
  /// True if the index file exists after the update, false if it
  /// doesn't (was deleted or didn't exist before and wasn't created).
  /// </returns>
  public bool Update()
  {
    if(!File.Exists(Owner.BlobsFileName))
    {
      if(File.Exists(FileName))
      {
        File.Delete(FileName);
      }
      ClearEntries();
      return false;
    }
    if(!File.Exists(FileName))
    {
      ClearEntries();
    }
    using var file = File.OpenWrite(FileName);
    if(file.Length < 8)
    {
      file.Write(BitConverter.GetBytes(Signature), 0, 8);
    }
    var expectedLength = 8 + _entries.Count*32;
    if(file.Length > expectedLength)
    {
      // There are more entries in the index file than we have in memory
      file.Seek(expectedLength, SeekOrigin.Begin);
      while(file.Position < file.Length)
      {
        // New entries appeared in the index file
        // This can happen if the BlobStorage was used by multiple
        // code paths or processes.
        // Or it can (will!) happen at startup.
        var entry = BlobEntry.TryRead(file);
        if(entry == null)
        {
          break;
        }
        _AppendEntry(entry);
      }
    }
    else if(file.Length < expectedLength)
    {
      // The index file does not contain all entries from memory
      // This is a logic error, indicating that something other
      // than Update() called _AppendEntry().
    }
    var blobTail = (long)(_entries.LastOrDefault()?.Tail ?? 8);
    if(blobTail > new FileInfo(Owner.BlobsFileName).Length)
    {
      // Desynced! Reset, prepare for full reload
      ClearEntries();
      file.Seek(8, SeekOrigin.Begin);
      file.SetLength(8);
      blobTail = 8;
    }
    // add new entries found in the blobs file
    using var blobsFile = File.OpenRead(Owner.BlobsFileName);
    blobsFile.Seek(blobTail, SeekOrigin.Begin);
    BlobEntry? blobEntry;
    while((blobEntry = BlobsFile.ReadAsBlobEntry(blobsFile))!=null)
    {
      _AppendEntry(blobEntry);
      blobEntry.Write(file);
    }
    return true;
  }

  /// <summary>
  /// Return any BlobEntries with a hash that starts with the given prefix.
  /// </summary>
  /// <param name="prefix">
  /// The hash prefix to find (case insensitive)
  /// </param>
  /// <returns>
  /// All matching BlobEntries
  /// </returns>
  public IEnumerable<BlobEntry> FindAllByPrefix(string prefix)
  {
    foreach(var entry in _entries)
    {
      if(entry.Hash.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
      {
        yield return entry;
      }
    }
  }

  /// <summary>
  /// Find the single BlobEntry with a hash that starts with the given
  /// unambiguous prefix. Throws an exception if multiple entries are found.
  /// </summary>
  /// <param name="prefix">
  /// The unambiguous hash prefix to find (case insensitive)
  /// </param>
  /// <returns>
  /// Null if none found, the single matching BlobEntry if found.
  /// Throws an exception if multiple entries are found.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown when multiple entries are found for the given prefix.
  /// </exception>
  public BlobEntry? FindOneByPrefix(string prefix)
  {
    var all = FindAllByPrefix(prefix).ToList();
    if(all.Count == 0)
    {
      return null;
    }
    if(all.Count > 1)
    {
      throw new InvalidOperationException(
        $"Multiple entries found for hash prefix {prefix}");
    }
    return all[0];
  }

  private void ClearEntries()
  {
    _entries.Clear();
    // Todo: clear index Dictionaries
  }

  /// <summary>
  /// Append a new entry to the index.
  /// MUST ONLY BE CALLED BY Update()!
  /// </summary>
  private void _AppendEntry(
    BlobEntry entry)
  {
    _entries.Add(entry);
    // Todo: update index Dictionaries
  }

  public const ulong Signature = 0x58444E49424F4C42; // BLOBINDX

}
