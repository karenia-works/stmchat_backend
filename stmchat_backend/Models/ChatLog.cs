using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using stmchat_backend.Models;

//此model并非为单一聊天条目的记录，而是群聊或是单聊的整个聊天过程的记录
//保存时长之后考虑，此model应当包含1/聊天记录编号2/聊天条目list（聊天条目仅作为字符串存储）
namespace stmchat_backend
{
    public class ChatLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public List<Message> Messages { get; set; }
    }
}