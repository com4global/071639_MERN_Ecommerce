using System.Text.Json;

namespace ApiOnLamda
{
    public class JsonDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
    {
        private readonly string _format;

        public JsonDateTimeConverter(string format)
        {
            _format = format;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }
}
