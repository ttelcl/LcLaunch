/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ttelcl.Persistence.API;

/// <summary>
/// Converts <see cref="TickId"/> into JSON and JSON into
/// <see cref="TickId"/>. Always writes in string format,
/// but can read both string format and long integers
/// </summary>
public class TickIdConverter: JsonConverter<TickId>
{

  /// <inheritdoc/>
  public override void WriteJson(
    JsonWriter writer,
    TickId value,
    JsonSerializer serializer)
  {
    writer.WriteValue(value.ToString());
  }

  /// <inheritdoc/>
  public override TickId ReadJson(
    JsonReader reader,
    Type objectType,
    TickId existingValue,
    bool hasExistingValue,
    JsonSerializer serializer)
  {
    if(reader.TokenType == JsonToken.Integer)
    {
      var ticks = (long)reader.Value!;
      return (TickId)ticks;
    }
    else if(reader.TokenType == JsonToken.String)
    {
      var str = (string)reader.Value!;
      return TickId.Parse(str);
    }
    else
    {
      throw new NotSupportedException(
        $"Cannot read a '{reader.TokenType}' as TickId (expecting string or integer)");
    }
  }
}
