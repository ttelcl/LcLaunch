/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcLauncher.Storage.BlobsStorage;

/// <summary>
/// Describes a single entry in a BlobStorage
/// </summary>
public class BlobEntry
{
  /// <summary>
  /// Create a new BlobEntry
  /// </summary>
  private BlobEntry(
    ulong offset,
    uint length,
    ReadOnlySpan<byte> hashSpan)
  {
    Offset = offset;
    Length = length;
    _hashBytes = new byte[20];
    hashSpan.CopyTo(_hashBytes);
    Hash = HashString(hashSpan);
  }

  public static string HashString(ReadOnlySpan<byte> hashSpan)
  {
    var hashBuilder = new StringBuilder(40);
    foreach(var b in hashSpan)
    {
      hashBuilder.Append(b.ToString("x2"));
    }
    return hashBuilder.ToString();
  }

  /// <summary>
  /// The offset of the blob in the blobs file
  /// (the offset of the length field)
  /// </summary>
  public ulong Offset { get; }

  /// <summary>
  /// The offset of the blob content in the blobs file
  /// </summary>
  public ulong ContentOffset => Offset + 4;

  /// <summary>
  /// The length of the blob in the blobs file
  /// </summary>
  public uint Length { get; }

  /// <summary>
  /// The offset of the next blob in the blobs file
  /// </summary>
  public ulong Tail => Offset + Length + 4;

  /// <summary>
  /// The SHA1 hash of the blob, as a lower case hex string of
  /// 40 characters
  /// </summary>
  public string Hash { get; }

  internal static BlobEntry Read(
    Span<byte> span)
  {
    var offset = BinaryPrimitives.ReadUInt64LittleEndian(span);
    var length = BinaryPrimitives.ReadUInt32LittleEndian(span[8..]);
    return new BlobEntry(offset, length, span.Slice(12, 20));
  }

  internal static BlobEntry? TryRead(
    Stream stream)
  {
    Span<byte> buffer = stackalloc byte[32];
    var n = stream.Read(buffer);
    if(n != 0 && n != 32)
    {
      throw new InvalidDataException(
        $"Corrupt index file. BlobEntry: expected 0 or 32 bytes, got {n}");
    }
    return n == 0 ? null : Read(buffer);
  }

  internal void Write(
    Stream stream)
  {
    Span<byte> buffer = stackalloc byte[32];
    BinaryPrimitives.WriteUInt64LittleEndian(buffer, Offset);
    BinaryPrimitives.WriteUInt32LittleEndian(buffer[8..], Length);
    _hashBytes.CopyTo(buffer[12..]);
    stream.Write(buffer);
  }

  /// <summary>
  /// The SHA1 hash of the blob, as a byte array (20 bytes)
  /// </summary>
  private byte[] _hashBytes;

}
