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
    [BsonKnownTypes(typeof(TextMsg), typeof(ImageMsg), typeof(FileMsg))]
    public class SendMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string sender { get; set; }
        public DateTime time { get; set; }
        public string forwardFrom { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string replyTo { get; set; }


    }
    [JsonDiscriminator("TextMsg")]
    // [BsonDiscriminator("TextMsg")]
    public class TextMsg : SendMessage
    {

        [BsonIgnoreIfNull]
        public string text { get; set; }
    }

    [BsonDiscriminator("file")]
    public class FileMsg : SendMessage
    {
        public string file { get; set; }
        public string filename { get; set; }

        public string caption { get; set; }//图片下面配文字
        public int size { get; set; }
    }
    [JsonDiscriminator("image")]
    public class ImageMsg : SendMessage
    {
        public string image { get; set; }
        public string caption { get; set; }
    }
    public class FowardProperty
    {
        public string userId { get; set; }
        public string username { get; set; }
        public string chatId { get; set; }
        public string msgId { get; set; }
    }
    public class RecvMessage
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string replyTo { get; set; }
    }
    [JsonDiscriminator("text")]
    public class RTextMsg : RecvMessage
    {
        public string text { get; set; }
    }
    [JsonDiscriminator("image")]
    public class RImageMsg : RecvMessage
    {
        public string image { get; set; }
        public string caption { get; set; }
    }
    [JsonDiscriminator("file")]
    public class RFileMsg : RecvMessage
    {
        public string file { get; set; }
        public string filename { get; set; }
        public string caption { get; set; }
        public int size { get; set; }
    }
    [JsonDiscriminator("forward")]
    public class RForwardMsg : RecvMessage
    {
        public string fromChatId { get; set; }
        public string fromMessageId { get; set; }
    }
    public class WsSendMsg
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string chatId { get; set; }
        public SendMessage msg { get; set; }
    }
    public class WsRecvMsg
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string chatId { get; set; }
        public RecvMessage msg { get; set; }
    }

}