using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;
using System.Text;
using Dahomey.Json;
using Dahomey.Json.Attributes;
using System.Text.Json;

namespace stmchat_backend.Models
{
    public class Message
    {
        public DateTime Time { get; set; }
        public string Sender { get; set; }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }

    [JsonDiscriminator("text")]
    public class TextMsg : Message
    {
        public string Text { get; set; }
    }

    [JsonDiscriminator("file")]
    public class FileMsg : Message
    {
        public string File { get; set; }
        public string Filename { get; set; }
        public int Size { get; set; }
        public string Caption { get; set; }
    }

    [JsonDiscriminator("image")]
    public class ImageMsg : Message
    {
        public string Image { get; set; }
        public string Caption { get; set; }
    }
}