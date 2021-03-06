﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace Shamyr.Cloud.Bson
{
  public class ObjectIdJsonConverter: JsonConverter<ObjectId>
  {
    public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
      var objectIdString = reader.GetString();

      if (ObjectId.TryParse(objectIdString, out var objectId))
        return objectId;

      return ObjectId.Empty;
      throw new FormatException($"Read string cannot be converted to type '{typeof(ObjectId)}'.");
    }

    public override void Write(Utf8JsonWriter writer, ObjectId value, System.Text.Json.JsonSerializerOptions options)
    {
      writer.WriteStringValue(value.ToString());
    }
  }
}
