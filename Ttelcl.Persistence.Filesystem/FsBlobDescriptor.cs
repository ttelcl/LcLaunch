/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ttelcl.Persistence.API;

namespace Ttelcl.Persistence.Filesystem;

/// <summary>
/// Describes a single entry in the Blob file; this content
/// is one entry in the blob index file.
/// </summary>
public class FsBlobDescriptor
{
  /// <summary>
  /// Create a new FsBlobDescriptor
  /// </summary>
  public FsBlobDescriptor(
    long offset,
    int size,
    ReadOnlySpan<byte> hash)
  {
    Size = size;
    Offset = offset;
    if(hash.Length != 20)
    {
      throw new ArgumentException(
        "Expecting blob hash to be 20 bytes in length");
    }
    var hashcopy = new byte[20];
    hash.CopyTo(hashcopy);
    Hash = hashcopy;
    Id = HashId.FromHash(hash);
  }

  internal static FsBlobDescriptor FromBytes(ReadOnlySpan<byte> bytes)
  {
    var offset = BinaryPrimitives.ReadInt64LittleEndian(bytes[..8]);
    var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[8..12]);
    var hash = bytes[12..32];
    return new FsBlobDescriptor(offset, size, hash);
  }

  /// <summary>
  /// The length of the blob
  /// </summary>
  public int Size { get; }

  /// <summary>
  /// The offset of the blob entry in the blobs file
  /// (the 4 byte size and the actual blob)
  /// </summary>
  public long Offset { get; }

  /// <summary>
  /// The 20 byte SHA1 hash of the blob. This library only handles
  /// small collections of blobs, relying on the first 8 bytes of the
  /// hash being unique.
  /// </summary>
  public IReadOnlyList<byte> Hash { get; }

  /// <summary>
  /// The ID for the blob (essentially the first 8 bytes of
  /// <see cref="Hash"/>).
  /// </summary>
  public HashId Id { get; }
}
