/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LcLauncher.Storage.BlobsStorage;

public class BlobsFile
{
  internal BlobsFile(
    BlobStorage owner)
  {
    Owner = owner;
  }

  public BlobStorage Owner { get; }

  public string FileName => Owner.BlobsFileName;

  public bool FileExists => File.Exists(FileName);

  public FileStream? OpenRead()
  {
    if(!File.Exists(FileName))
    {
      return null;
    }
    return File.OpenRead(FileName);
  }

  /// <summary>
  /// Oppen the file for writing (creating it if necessary)
  /// and seek to the end of the file, ready for appending.
  /// </summary>
  /// <returns></returns>
  public FileStream OpenAppend()
  {
    var file = File.OpenWrite(FileName);
    if(file.Length == 0)
    {
      file.Write(BitConverter.GetBytes(Signature));
    }
    file.Seek(0, SeekOrigin.End);
    return file;
  }

  public const ulong Signature = 0x524F5453424F4C42; // BLOBSTOR
  public const int HeaderSize = 8;

  public static BlobEntry? ReadAsBlobEntry(Stream file)
  {
    Span<byte> buffer = stackalloc byte[32];
    BinaryPrimitives.WriteInt64LittleEndian(buffer, file.Position);
    var n = file.Read(buffer.Slice(8, 4));
    if(n != 4)
    {
      if(n != 0)
      {
        throw new InvalidDataException(
          $"Corrupt blobs file. BlobFileEntry: expected 0 or 4 bytes, got {n}");
      }
      return null;
    }
    var length = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(8, 4));
    // TODO: use sub-streams to avoid reading the whole blob content into memory
    var content = new byte[length];
    file.Read(content);
    SHA1.HashData(content, buffer.Slice(12, 20));
    return BlobEntry.Read(buffer);
  }

  public BlobEntry? ReadAsBlobEntry(long offset)
  {
    using var file = OpenRead();
    if(file == null)
    {
      return null;
    }
    file.Seek(offset, SeekOrigin.Begin);
    return ReadAsBlobEntry(file);
  }

  internal BlobEntry AppendBlob(byte[] blob, ReadOnlySpan<byte> hash)
  {
    using var file = OpenAppend();
    var offset = file.Position;
    Span<byte> buffer = stackalloc byte[32];
    BinaryPrimitives.WriteInt64LittleEndian(buffer, offset);
    BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(8, 4), blob.Length);
    hash.CopyTo(buffer.Slice(12, 20));
    file.Write(buffer.Slice(8,4));
    file.Write(blob);
    return BlobEntry.Read(buffer);
  }

  public BlobEntry AppendBlob(byte[] blob)
  {
    Span<byte> hash = stackalloc byte[20];
    SHA1.HashData(blob, hash);
    return AppendBlob(blob, hash);
  }

  /// <summary>
  /// Undo the last AppendBlob operation, truncating the blobs file
  /// to the offset of the last blob written. This is useful if the
  /// entry turns out to be a duplicate.
  /// </summary>
  /// <param name="entryJustWritten">
  /// The blob entry that was just written and should be removed.
  /// </param>
  public void CancelAppend(BlobEntry entryJustWritten)
  {
    using var file = OpenAppend();
    file.SetLength((long)entryJustWritten.Offset);
  }

  public static byte[] ReadBlob(Stream file, long offset, int length)
  {
    if(length<0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(length), "Blob Length must be non-negative");
    }
    if(offset<8)
    {
      throw new ArgumentOutOfRangeException(
        nameof(offset), "Blob Offset must be at least 8");
    }
    if(offset+length>file.Length)
    {
      throw new ArgumentOutOfRangeException(
        nameof(length), "Blob Offset+Length exceeds file size");
    }
    file.Seek(offset, SeekOrigin.Begin);
    var buffer = new byte[length];
    var n = file.Read(buffer);
    if(n != length)
    {
      throw new InvalidDataException(
        $"Corrupt blobs file. ReadBlob: expected {length} bytes, got {n}");
    }
    return buffer;
  }

  public static byte[] ReadBlob(Stream file, BlobEntry entry)
  {
    return ReadBlob(file, (long)entry.ContentOffset, (int)entry.Length);
  }

  public static byte[] ReadBlob(Stream file, long offset)
  {
    file.Seek(offset, SeekOrigin.Begin);
    Span<byte> buffer = stackalloc byte[4];
    if(file.Read(buffer)!=4)
    {
      throw new InvalidDataException(
        "Corrupt blobs file. ReadBlob: expected 4 bytes for length");
    }
    var length = BinaryPrimitives.ReadInt32LittleEndian(buffer);
    return ReadBlob(file, offset, length);
  }

  public static byte[] ReadBlob(Stream file)
  {
    return ReadBlob(file, file.Position);
  }

  public byte[] ReadBlob(BlobEntry entry)
  {
    using var file =
      OpenRead() ?? throw new FileNotFoundException(
        "Blobs file not found", FileName);
    return ReadBlob(file, entry);
  }

  public byte[] ReadBlob(long offset, int length)
  {
    using var file =
      OpenRead() ?? throw new FileNotFoundException(
        "Blobs file not found", FileName);
    return ReadBlob(file, offset, length);
  }

  public byte[] ReadBlob(long offset)
  {
    using var file =
      OpenRead() ?? throw new FileNotFoundException(
        "Blobs file not found", FileName);
    return ReadBlob(file, offset);
  }
}
