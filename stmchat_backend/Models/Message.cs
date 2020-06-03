using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;
using System.Text;

namespace stmchat_backend.Models
{
    public class Message
    {
        public DateTime time { get; set; }
        public string sender { get; set; }
        [BsonId]
        public string id { get; set; }

    }
    public class TextMsg : Message
    {
        public string text { get; set; }
    }
    public class FileMsg : Message
    {
        public string file { get; set; }
        public string filename { get; set; }
        public string caption { get; set; }
    }
    public class ImageMsg : Message
    {
        public string image { get; set; }
        public string caption { get; set; }
    }
}