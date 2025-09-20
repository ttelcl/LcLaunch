/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Ttelcl.Persistence.API;

/// <summary>
/// A 'hopefully unique' identifier for a blob, based
/// on the first 8 bytes (63 bits, really) of its SHA1 hash
/// </summary>
[JsonConverter(typeof(HashIdConverter))]
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
    // Exceptionally, use BigEndian, to stay somewhat compatible
    // with the older implementation. Or at least make errors more
    // discoverable.
    var l =
      BinaryPrimitives.ReadInt64BigEndian(hashBytes)
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
  /// Also returns the full hash in binary form
  /// </summary>
  /// <param name="blob"></param>
  /// <param name="fullHash"></param>
  /// <returns></returns>
  public static HashId FromBlob(
    ReadOnlySpan<byte> blob,
    out byte[] fullHash)
  {
    fullHash = new byte[20];
    SHA1.HashData(blob, fullHash);
    return FromHash(fullHash);
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

  /// <summary>
  /// Returns the hexadecimal representation of this <see cref="HashId"/>
  /// </summary>
  /// <returns></returns>
  public override string ToString()
  {
    return _id.ToString("X16");
  }

  /// <summary>
  /// Parse the <see cref="HashId"/> from its hexadecimal representation
  /// </summary>
  /// <param name="s"></param>
  /// <returns></returns>
  /// <exception cref="ArgumentException"></exception>
  public static HashId Parse(string s)
  {
    if(s.Length != 16 || !Int64.TryParse(
      s,
      NumberStyles.AllowHexSpecifier,
      CultureInfo.InvariantCulture,
      out var l))
    {
      throw new ArgumentException(
        $"Expecting a 16 character hexedecimal string but got '{s}'");
    }
    return (HashId)l;
  }
}
