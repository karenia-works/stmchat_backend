using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;
using System.Text;

namespace stmchat_backend.Models
{
    public class Message
    {
        public string _t { get; set; }
        public DateTime Time { get; set; }
        [BsonId] public string FromId { get; set; }
        public string FromName { get; set; }
        public string Content { get; set; }
    }
}