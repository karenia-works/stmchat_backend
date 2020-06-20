//群聊相关，包括群号，群id，群成员（list），群聊天记录号（ChatLog）

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace stmchat_backend.Models
{
    public class ChatGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string id { get; set; }
        public string name { get; set; }//搜索用的群名类似用户名
        public Boolean isFriend { get; set; }
        public string AvatarUrl { get; set; }
        public string owner { get; set; }
        public string describ { get; set; }//群描述
        public List<String> members { get; set; }//群成员,存储username!!
        [BsonRepresentation(BsonType.ObjectId)]
        public string chatlog { get; set; }//一一对应的聊天记录

        [JsonIgnore]
        public Dictionary<string, ObjectId> UserLatestRead { get; set; }
    }
}
