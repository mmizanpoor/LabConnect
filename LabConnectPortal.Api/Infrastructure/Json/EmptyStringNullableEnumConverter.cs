using System.Text.Json;
using System.Text.Json.Serialization;

namespace LabConnectPortal.Api.Infrastructure.Json;

/// <summary>
/// Treats empty/whitespace JSON strings as null for nullable enums
/// (avoids HTTP 400 when clients send <c>status: ""</c> for "all").
/// </summary>
public sealed class EmptyStringNullableEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        var underlying = Nullable.GetUnderlyingType(typeToConvert);
        return underlying is { IsEnum: true };
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var enumType = Nullable.GetUnderlyingType(typeToConvert)!;
        var converterType = typeof(EmptyStringNullableEnumConverter<>).MakeGenericType(enumType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class EmptyStringNullableEnumConverter<TEnum> : JsonConverter<TEnum?>
        where TEnum : struct, Enum
    {
        public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                    return null;

                if (Enum.TryParse<TEnum>(raw, ignoreCase: true, out var byName))
                    return byName;

                throw new JsonException($"Unable to convert \"{raw}\" to enum {typeof(TEnum).Name}.");
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var number))
                return (TEnum)Enum.ToObject(typeof(TEnum), number);

            throw new JsonException($"Unexpected token {reader.TokenType} when parsing nullable enum {typeof(TEnum).Name}.");
        }

        public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Value.ToString());
        }
    }
}
