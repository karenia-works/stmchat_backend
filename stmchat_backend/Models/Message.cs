using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;
using System.Text;
using Dahomey.Json;
using Dahomey.Json.Attributes;
using System.Text.Json;
using System.Collections.Generic;

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
        public FowardProperty forwardFrom { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public SendMessage replyTo { get; set; }


    }
    [JsonDiscriminator("text_s")]
    [BsonDiscriminator("text_s")]
    public class TextMsg : SendMessage
    {

        [BsonIgnoreIfNull]
        public string text { get; set; }
    }
    [JsonDiscriminator("file_s")]
    [BsonDiscriminator("file_s")]
    public class FileMsg : SendMessage
    {
        public string file { get; set; }
        public string filename { get; set; }

        public string caption { get; set; }//图片下面配文字
        public int size { get; set; }
    }
    [JsonDiscriminator("image_s")]
    [BsonDiscriminator("image_s")]
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
    [BsonKnownTypes(typeof(RTextMsg), typeof(RImageMsg), typeof(RFileMsg))]
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
        public string Image { get; set; }
        public string Caption { get; set; }
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

#nullable enable
    [BsonKnownTypes(typeof(WsSendChatMsg), typeof(WsSendUnreadCountMsg), typeof(WsSendOnlineStatusMsg))]
    public class WsSendMsg
    {
        // HACK: ID 只是用来调试的时候追踪来往消息的，没有任何实质作用
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
        public string? replyTo { get; set; }
    }
#nullable restore

    [JsonDiscriminator("chat_s")]
    public class WsSendChatMsg : WsSendMsg
    {
        public string chatId { get; set; }
        public SendMessage msg { get; set; }
    }

    [JsonDiscriminator("unread_s")]
    public class WsSendUnreadCountMsg : WsSendMsg
    {
        public Dictionary<string, UnreadProperty> items { get; set; }
    }

    public struct UnreadProperty
    {
        public int count { get; set; }
        public ObjectId maxMessage { get; set; }
    }

    [JsonDiscriminator("online_s")]
    public class WsSendOnlineStatusMsg : WsSendMsg
    {
        public string userId { get; set; }
        public bool online { get; set; }
    }

    [JsonDiscriminator("err_s")]
    public class WsSendErrMsg : WsSendMsg
    {
        public String error { get; set; }
    }

#nullable enable
    public class WsRecvMsg
    {
        public string? id { get; set; }
        public string? replyTo { get; set; }
    }
#nullable restore

    [JsonDiscriminator("chat")]
    public class WsRecvChatMsg : WsRecvMsg
    {
        //  [BsonRepresentation(BsonType.ObjectId)]
        public string chatId { get; set; }
        public RecvMessage msg { get; set; }
    }

    [JsonDiscriminator("read_position")]
    public class WsRecvReadPositionMsg : WsRecvMsg
    {
        public string chatId { get; set; }
        public string msgId { get; set; }
    }

}
