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
        public DateTime time { get; set; }
        public string sender { get; set; }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

    }
    [JsonDiscriminator("text")]
    public class TextMsg : Message
    {
        public string text { get; set; }
    }
    [JsonDiscriminator("file")]
    public class FileMsg : Message
    {
        public string file { get; set; }
        public string filename { get; set; }
        public int size { get; set; }
        public string caption { get; set; }
    }
    [JsonDiscriminator("image")]
    public class ImageMsg : Message
    {
        public string image { get; set; }
        public string caption { get; set; }
    }

}