using System;
using Newtonsoft.Json;
using SixLabors.ImageSharp.PixelFormats;

namespace Matrix;

/// <summary>
/// Reads and writes Rgba32 values as hex strings.
/// </summary>
public class Rgba32JsonConverter : JsonConverter<Rgba32>
{
    public override Rgba32 ReadJson(JsonReader reader, Type objectType, Rgba32 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var hex = (string)(reader.Value ?? throw new JsonSerializationException("Expected string"));
            return Rgba32.ParseHex(hex);
        }
        throw new JsonSerializationException("Expected string");
    }

    public override void WriteJson(JsonWriter writer, Rgba32 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToHex());
    }
}
