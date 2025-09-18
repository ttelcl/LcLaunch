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

namespace Ttelcl.Persistence.API;

/// <summary>
/// Serializes and deserializes <see cref="HashId"/> instances
/// as a 16 character hexadecimal string. Also allows JSON integers
/// on deserialization
/// </summary>
public class HashIdConverter: JsonConverter<HashId>
{

  /// <inheritdoc/>
  public override void WriteJson(
    JsonWriter writer,
    HashId value,
    JsonSerializer serializer)
  {
    writer.WriteValue(value.ToString());
  }

  /// <inheritdoc/>
  public override HashId ReadJson(
    JsonReader reader,
    Type objectType,
    HashId existingValue,
    bool hasExistingValue,
    JsonSerializer serializer)
  {
    if(reader.TokenType == JsonToken.Integer)
    {
      var value = (long)reader.Value!;
      return (HashId)value;
    }
    else if(reader.TokenType == JsonToken.String)
    {
      var str = (string)reader.Value!;
      return HashId.Parse(str);
    }
    else
    {
      throw new NotSupportedException(
        $"Cannot read a '{reader.TokenType}' as HashId (expecting string or integer)");
    }
  }

}
