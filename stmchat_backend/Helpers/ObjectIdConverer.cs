using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace stmchat_backend.Helpers
{
    public class ObjectIdConverter : JsonConverter<ObjectId>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(ObjectId);
        }

        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            return new ObjectId(s);
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
