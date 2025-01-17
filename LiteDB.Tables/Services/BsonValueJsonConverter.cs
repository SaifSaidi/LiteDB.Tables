
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LiteDB;

public class BsonValueJsonConverter : JsonConverter<BsonValue>
{
    public override BsonValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization is not supported");
    }

    public override void Write(Utf8JsonWriter writer, BsonValue value, JsonSerializerOptions options)
    {
        switch (value.Type)
        {
            case BsonType.Null:
                writer.WriteNullValue();
                break;
            case BsonType.Int32:
                writer.WriteNumberValue(value.AsInt32);
                break;
            case BsonType.Int64:
                writer.WriteNumberValue(value.AsInt64);
                break;
            case BsonType.Decimal:
                writer.WriteNumberValue(value.AsDecimal);
                break;
            case BsonType.Double:
                writer.WriteNumberValue(value.AsDouble);
                break;
            case BsonType.String:
                writer.WriteStringValue(value.AsString);
                break;
            case BsonType.Boolean:
                writer.WriteBooleanValue(value.AsBoolean);
                break;
            case BsonType.DateTime:
                writer.WriteStringValue(value.AsDateTime.ToString("o")); // ISO 8601 format
                break;
            case BsonType.Array:
                writer.WriteStartArray();
                foreach (var item in value.AsArray)
                {
                    System.Text.Json.JsonSerializer.Serialize(writer, item, options); // Recursive serialization
                }
                writer.WriteEndArray();
                break;
            case BsonType.Document:
                writer.WriteStartObject();
                foreach (var item in value.AsDocument)
                {
                    writer.WritePropertyName(item.Key);
                    System.Text.Json.JsonSerializer.Serialize(writer, item.Value, options); // Recursive serialization
                }
                writer.WriteEndObject();
                break;
            case BsonType.ObjectId:
                writer.WriteStringValue(value.AsObjectId.ToString());
                break;
            default:
                throw new JsonException($"Unsupported BsonType: {value.Type}");
        }
    }
}
