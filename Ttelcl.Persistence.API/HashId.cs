/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ttelcl.Persistence.API;

/// <summary>
/// A 'hopefully unique' identifier for a blob, based
/// on its SHA1 hash
/// </summary>
public record struct HashId
{
  private readonly long _id;

  /// <summary>
  /// Create a new HashId
  /// </summary>
  public HashId()
  {
    _id = default;
  }

  internal HashId(long id)
  {
    _id = id;
  }

  /// <summary>
  /// Create a <see cref="HashId"/> from the first 8 bytes
  /// of <paramref name="hashBytes"/>, and remove the sign bit.
  /// </summary>
  /// <param name="hashBytes">
  /// The bytes containing the result of the hash calculation
  /// </param>
  /// <returns></returns>
  /// <exception cref="ArgumentException"></exception>
  public static HashId FromHash(ReadOnlySpan<byte> hashBytes)
  {
    if(hashBytes.Length < 8)
    {
      throw new ArgumentException(
        "Expecting a hash of at least 8 bytes");
    }
    var l =
      BinaryPrimitives.ReadInt64LittleEndian(hashBytes)
      & Int64.MaxValue;
    return new HashId(l);
  }

  /// <summary>
  /// Create a <see cref="HashId"/> from the SHA1 hash of the blob.
  /// Also return the full hash in text form
  /// </summary>
  /// <param name="blob"></param>
  /// <param name="fullHash"></param>
  /// <returns></returns>
  public static HashId FromBlob(
    ReadOnlySpan<byte> blob,
    out string fullHash)
  {
    Span<byte> hash = stackalloc byte[20];
    SHA1.HashData(blob, hash);
    var sb = new StringBuilder(40);
    foreach(var b in hash)
    {
      _ = sb.AppendFormat("x2", b);
    }
    fullHash = sb.ToString();
    return FromHash(hash);
  }

  /// <summary>
  /// Create a <see cref="HashId"/> from the SHA1 hash of the blob.
  /// </summary>
  /// <param name="blob"></param>
  /// <returns></returns>
  public static HashId FromBlob(
    ReadOnlySpan<byte> blob)
  {
    Span<byte> hash = stackalloc byte[20];
    SHA1.HashData(blob, hash);
    return FromHash(hash);
  }

  /// <summary>
  /// Implicit conversion from <see cref="long"/> to <see cref="HashId"/>
  /// </summary>
  /// <param name="id"></param>
  public static implicit operator HashId(long id)
  {
    return new HashId(id);
  }

  /// <summary>
  /// Implicit conversion from <see cref="HashId"/> to <see cref="long"/>
  /// </summary>
  /// <param name="hid"></param>
  public static implicit operator long(HashId hid)
  {
    return hid._id;
  }
}
