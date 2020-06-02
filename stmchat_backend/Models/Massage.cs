using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;
using System.Text;
namespace stmchat_backend.Models
{
    public class massage
    {
        public string _t { get; set; }
        public DateTime time { get; set; }
        [BsonId]
        public string fromid { get; set; }
        public string fromname { get; set; }
        public string content { get; set; }
    }
}