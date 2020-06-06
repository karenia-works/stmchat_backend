//群聊相关，包括群号，群id，群成员（list），群聊天记录号（ChatLog）

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace stmchat_backend.Models
{
    public class ChatGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string GroupName { get; set; } //搜索用的群名类似用户名
        public string Description { get; set; } //群描述
        public List<String> Members { get; set; } //群成员,存储username!!
        [BsonId] public string Chatlog { get; set; } //一一对应的聊天记录
    }
}