using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgencyLayer.Orchestration.Checkpointing;

/// <summary>
/// A JSON converter that converts <see cref="JsonElement"/> values to native CLR types
/// when deserializing into <see cref="object"/>. Without this converter,
/// <c>JsonSerializer.Deserialize&lt;Dictionary&lt;string, object&gt;&gt;()</c> produces
/// <see cref="JsonElement"/> values instead of <see cref="string"/>, <see cref="long"/>, etc.
/// </summary>
public class JsonElementObjectConverter : JsonConverter<object>
{
    /// <inheritdoc/>
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number when reader.TryGetInt64(out var l) => l,
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.String when reader.TryGetDateTime(out var dt) => dt,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.StartArray => ReadArray(ref reader, options),
            JsonTokenType.StartObject => ReadObject(ref reader, options),
            JsonTokenType.Null => null,
            _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    private static List<object?> ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var list = new List<object?>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var converter = (JsonConverter<object>)options.GetConverter(typeof(object));
            list.Add(converter.Read(ref reader, typeof(object), options));
        }
        return list;
    }

    private static Dictionary<string, object?> ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var dict = new Dictionary<string, object?>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var key = reader.GetString()!;
            reader.Read();
            var converter = (JsonConverter<object>)options.GetConverter(typeof(object));
            dict[key] = converter.Read(ref reader, typeof(object), options);
        }
        return dict;
    }
}
