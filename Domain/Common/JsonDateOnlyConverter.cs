using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Common
{
    public class JsonDateOnlyConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            string? dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
                return null;

            if (DateTime.TryParse(dateString, out DateTime result))
                return result.Date;

            throw new JsonException($"Unable to parse date string: {dateString}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd")); 
            else
                writer.WriteNullValue();
        }
    }
}
