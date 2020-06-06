using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

//此model包含用户信息
//应当包括用户id，用户邮箱，可能的简单用户信息，用户好友编号组（list），用户好友聊天记录组（list）顺序对应，群聊编号组，用户群聊聊天记录组（list）

namespace stmchat_backend.Models
{
    public class Profile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { set; get; }

        public string Email { set; get; }
        public string Username { set; get; }
        public string AvatarUrl { set; get; }
        public List<string> Friends { set; get; }
        public List<string> Groups { set; get; }
    }
}