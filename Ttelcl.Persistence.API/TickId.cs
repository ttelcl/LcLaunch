/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Ttelcl.Persistence.API;

/// <summary>
/// An object that has a <see cref="TickId"/> as identifier
/// </summary>
public interface IHasTickId
{
  /// <summary>
  /// The ID of the object
  /// </summary>
  TickId Id { get; }
}

/// <summary>
/// Locally unique identifiers, fitting in a 64 bit signed integer
/// </summary>
[JsonConverter(typeof(TickIdConverter))]
public record struct TickId
{
  private readonly long _id;
  // Ticks at 1970-01-01 00:00:00
  private const long __offset = 0x089F7FF5F7B58000L;
  private static long __lastValue =
    DateTimeOffset.UtcNow.Ticks - __offset;
  private static object __lock = new(); // System.Threading.Lock requires .net9

  /// <summary>
  /// Return a new <see cref="TickId"/> different from any returned
  /// by this method before. Thread safe.
  /// </summary>
  /// <returns></returns>
  public static TickId New()
  {
    lock(__lock)
    {
      var eticks = DateTimeOffset.UtcNow.Ticks - __offset;
      if(eticks <= __lastValue)
      {
        eticks = __lastValue+1L;
      }
      __lastValue = eticks;
      return new TickId(eticks);
    }
  }

  /// <summary>
  /// The null <see cref="TickId"/>
  /// </summary>
  public static readonly TickId Zero = new(0L);

  /// <summary>
  /// Explicitly initialized <see cref="TickId"/> (for cloning
  /// existing IDs)
  /// </summary>
  /// <param name="id"></param>
  public TickId(long id)
  {
    _id = id;
  }

  /// <summary>
  /// Convert a <see cref="long"/> to a <see cref="TickId"/>
  /// (implicit)
  /// </summary>
  public static implicit operator TickId(long eticks)
  {
    return new TickId(eticks);
  }

  /// <summary>
  /// Convert a <see cref="TickId"/> to a <see cref="long"/>
  /// (implicit)
  /// </summary>
  public static implicit operator long(TickId id)
  {
    return id._id;
  }

  /// <summary>
  /// Convert a <see cref="DateTimeOffset"/> to a <see cref="TickId"/>
  /// </summary>
  public static implicit operator TickId(DateTimeOffset dto)
  {
    return new TickId(
      dto.UtcTicks - __offset);
  }

  /// <summary>
  /// Convert a <see cref="TickId"/> to a <see cref="DateTimeOffset"/>
  /// (explicit)
  /// </summary>
  public static explicit operator DateTimeOffset(TickId id)
  {
    return new DateTimeOffset(id._id + __offset, TimeSpan.Zero);
  }

  /// <summary>
  /// Convert a <see cref="DateTimeOffset"/> to a <see cref="TickId"/>.
  /// <paramref name="timestamp"/> must be an UTC timestamp
  /// (<see cref="DateTime.Kind"/> must be <see cref="DateTimeKind.Utc"/>)
  /// </summary>
  public static implicit operator TickId(DateTime timestamp)
  {
    if(timestamp.Kind != DateTimeKind.Utc)
    {
      throw new ArgumentException(
        "Converting a DateTime to a TickId requires an UTC DateTime");
    }
    return new TickId(
      timestamp.Ticks - __offset);
  }

  /// <summary>
  /// Convert a <see cref="TickId"/> to a UTC <see cref="DateTime"/>.
  /// </summary>
  /// <param name="id"></param>
  public static explicit operator DateTime(TickId id)
  {
    return new DateTime(id._id + __offset, DateTimeKind.Utc);
  }

  /// <summary>
  /// Convert this <see cref="TickId"/> to a string, in a form
  /// representing the underlying UTC timestamp.
  /// </summary>
  /// <returns></returns>
  public override string ToString()
  {
    var dto = (DateTimeOffset)this;
    return dto.ToString("yyyyMMdd-HHmmss-fffffff");
  }

  /// <summary>
  /// Try parsing the string as a TickId timestamp format
  /// (yyyyMMdd-HHmmss-fffffff).
  /// </summary>
  /// <param name="str"></param>
  /// <param name="tickId"></param>
  /// <returns></returns>
  public static bool TryParse(string str, out TickId tickId)
  {
    if(!Regex.IsMatch(str, @"^\d{8}-\d{6}-\d{7}$"))
    {
      tickId = default(TickId);
      return false;
    }
    var dto = DateTimeOffset.ParseExact(
      str,
      "yyyyMMdd-HHmmss-fffffff",
      CultureInfo.InvariantCulture,
      DateTimeStyles.AssumeUniversal
      | DateTimeStyles.AdjustToUniversal);
    tickId = dto;
    return true;
  }

  /// <summary>
  /// Parse a string in TickId timestamp format
  /// (yyyyMMdd-HHmmss-fffffff). 
  /// </summary>
  /// <param name="str"></param>
  /// <returns></returns>
  /// <exception cref="ArgumentException"></exception>
  public static TickId Parse(string str)
  {
    if(!TryParse(str, out var tickId))
    {
      throw new ArgumentException(
        $"Expecting a string in 'yyyyMMdd-HHmmss-fffffff' format," +
        $" but got '{str}'");
    }
    return tickId;
  }

}
