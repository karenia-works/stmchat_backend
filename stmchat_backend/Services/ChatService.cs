//聊天相关，先放着
using System;
using MongoDB.Bson;
using MongoDB.Driver;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using stmchat_backend.Models;
using stmchat_backend.Services;
using Dahomey.Json;
using System.Text.Json;
using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Attributes;
using stmchat_backend.Helpers;
using System.Net.WebSockets;
using System.Collections;
using stmchat_backend.Models.Settings;

using MongoDB.Driver.Linq;

namespace stmchat_backend
{
    public class ChatService
    {
        public Dictionary<string, JsonWebsocketWrapper<WsRecvMsg, WsSendMsg>> Wsmap;
        public Dictionary<string, List<string>> Groupmap;
        public Dictionary<string, Dictionary<string, int>> notreadmap;//人，群
        public IMongoCollection<ChatLog> _chatlog;
        private IMongoCollection<ChatGroup> _groups;
        private IMongoDatabase database;

        public ChatService(IDbSettings settings)
        {

            var client = new MongoClient(settings.DbConnection);
            database = client.GetDatabase(settings.DbName);
            _chatlog = database.GetCollection<ChatLog>(settings.ChatLogCollectionName);
            _groups = database.GetCollection<ChatGroup>(settings.ChatGroupCollectionName);
            Wsmap = new Dictionary<string, JsonWebsocketWrapper<WsRecvMsg, WsSendMsg>>();
            notreadmap = new Dictionary<string, Dictionary<string, int>>();
        }

        public async Task<JsonWebsocketWrapper<WsRecvMsg, WsSendMsg>> Addsocket(String name, WebSocket webSocket, JsonSerializerOptions jsonSerializer)
        {

            var tgt = new JsonWebsocketWrapper<WsRecvMsg, WsSendMsg>(webSocket, jsonSerializer);

            Wsmap.Add(name, tgt);
            var unread = await getUnreadMsg(name);
            if (unread.Count != 0)
            {
                foreach (var item in unread)
                {
                    await tgt.SendMessage(item);
                }
            }
            tgt.Messages.Subscribe(
                          (msg) => { dealMsg(name, msg); },
                           (err) => { Console.WriteLine("err: {0}", err); },
                           () => { Wsmap.Remove(name); });
            return tgt;
        }

        public async void dealMsg(string name, WsRecvMsg recv)
        {
            Console.WriteLine(recv.msg.GetType());

            var groupId = recv.chatId;
            var group = await FindGroup(groupId);
            var groupname = group.chatlog;
            var members = group.members;
            var logid = group.chatlog;
            var msg = TransWsMsg(name, recv);

            InsertChat(logid, msg);
            foreach (var men in members)
            {
                if (Wsmap.ContainsKey(men))
                {
                    await Wsmap[men].SendMessage(msg);
                }
                else
                {
                    if (notreadmap.ContainsKey(men))
                    {
                        var tmpgroup = notreadmap[men];
                        if (tmpgroup.ContainsKey(groupname))
                        {
                            tmpgroup[groupname]++;
                        }
                        else
                            tmpgroup.Add(groupname, 1);
                    }
                    else
                    {
                        var tmpgroup = new Dictionary<string, int>();
                        tmpgroup.Add(groupname, 1);
                        notreadmap.Add(men, tmpgroup);
                    }
                }
            }
        }

        public async Task<List<WsSendMsg>> getUnreadMsg(string name)
        {
            var allunread = new List<WsSendMsg>();
            if (notreadmap.ContainsKey(name) == false)
            {
                notreadmap.Add(name, new Dictionary<string, int>());
            }

            var unreads = notreadmap[name];
            foreach (var item in unreads)
            {
                if (item.Value != 0)
                { allunread.AddRange(await getGroupMsg(item.Key, item.Value)); }
            }
            unreads.Remove(name);
            return allunread;
        }

        public WsSendMsg TransWsMsg(string name, WsRecvMsg rwsmsg)
        {
            SendMessage tgt = null;
            if (rwsmsg.msg.GetType() == typeof(RTextMsg))
                tgt = TransMsg(name, rwsmsg.msg as RTextMsg);
            var swsmsg = new WsSendMsg()
            {
                chatId = rwsmsg.chatId,
                msg = tgt
            };
            return swsmsg;
        }

        public SendMessage TransMsg(string name, RecvMessage tgt)
        {
            return new SendMessage();
        }

        public TextMsg TransMsg(string name, RTextMsg tgt)
        {
            Console.WriteLine("is text");
            var msg = new TextMsg()
            {
                id = ObjectId.GenerateNewId().ToString(),
                sender = name,
                time = DateTime.Now,
                text = tgt.text

            };

            return msg;
        }

        public SendMessage TransMsg(string name, RFileMsg tgt)
        {
            return new SendMessage();
        }

        public async void SendAll(List<JsonWebsocketWrapper<WsRecvMsg, WsSendMsg>> clo, WsSendMsg Message)
        {
            foreach (var item in clo)
            {
                await item.SendMessage(Message);
            }
        }

        public async Task<ChatGroup> FindGroup(string groupname)
        {
            var tgt = await _groups.AsQueryable().Where(o => o.name == groupname).FirstOrDefaultAsync();
            return tgt;
        }

        public async void InsertChat(string chatlog, WsSendMsg sendMsg)
        {
            var flicker = Builders<ChatLog>.Filter.Eq("id", chatlog);
            var update = Builders<ChatLog>.Update.Push(o => o.messages, sendMsg);

            await _chatlog.UpdateOneAsync(flicker, update);
        }

        public async Task<List<WsSendMsg>> getGroupMsg(string logid, int num)
        {

            var msgs = await _chatlog.AsQueryable().Where(o => o.id == logid).SelectMany(o => o.messages).ToListAsync();
            var res = msgs.TakeLast(num).ToList();
            return res;//粪代码
        }

        public async Task<WsSendMsg> getMsg(string logid, string msgid)
        {

            var msg = await _chatlog.AsQueryable().Where(o => o.id == logid).SelectMany(o => o.messages).Where(o => o.msg.id == msgid).FirstOrDefaultAsync();
            return msg;

        }

        public async void insert()
        {
            var res = new ChatLog() { id = ObjectId.GenerateNewId().ToString(), messages = new List<WsSendMsg>() };
            var m1 = new WsSendMsg()
            {
                chatId = ObjectId.GenerateNewId().ToString(),
                msg = new TextMsg()
                {
                    id = ObjectId.GenerateNewId().ToString(),
                    sender = "he",
                    time = DateTime.Now,
                    forwardFrom = "sssdd",
                    replyTo = ObjectId.GenerateNewId().ToString(),
                    text = "i am a text"
                }
            };
            var m2 = new WsSendMsg()
            {
                chatId = ObjectId.GenerateNewId().ToString(),
                msg = new TextMsg()
                {
                    id = ObjectId.GenerateNewId().ToString(),
                    sender = "xing",
                    time = DateTime.Now,
                    forwardFrom = "sssdd",
                    replyTo = ObjectId.GenerateNewId().ToString(),
                    text = "text fuck"
                }
            };
            var m3 = new WsSendMsg()
            {
                chatId = ObjectId.GenerateNewId().ToString(),
                msg = new TextMsg()
                {
                    id = ObjectId.GenerateNewId().ToString(),
                    sender = "yu",
                    time = DateTime.Now,
                    forwardFrom = "sssdd",
                    replyTo = ObjectId.GenerateNewId().ToString(),
                    text = "text fuck"
                }
            };
            res.messages.Add(m1);
            res.messages.Add(m2);
            res.messages.Add(m3);
            await _chatlog.InsertOneAsync(res);
        }

        private async void ProcessReadPositionMessage(string username)
        {

        }

        private async Task<long> UpdateAndCountUnreadMessage(string groupId, string userId, ObjectId messageId)
        {
            await this._groups.FindOneAndUpdateAsync(
                new FilterDefinitionBuilder<ChatGroup>().Where(group => group.name == groupId),
                new UpdateDefinitionBuilder<ChatGroup>().Max((group) => group.UserLatestRead[userId], messageId));

            var group = await this._groups.Find(
                new FilterDefinitionBuilder<ChatGroup>().Where(group => group.name == groupId))
                .SingleAsync();

            var count = await this.database.GetCollection<SendMessage>(group.chatlog).CountDocumentsAsync(
                new FilterDefinitionBuilder<SendMessage>().Where(msg => msg.id.CompareTo(messageId.ToString()) > 0)
            );

            return count;
        }
    }
}
