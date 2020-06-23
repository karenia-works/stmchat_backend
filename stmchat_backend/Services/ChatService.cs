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
using Microsoft.Extensions.Logging;
using System.Threading;

namespace stmchat_backend
{
    public class ChatService
    {
        public Dictionary<string, JsonWebsocketWrapper<WsRecvMsg, WsSendMsg>> WsCastMap;

        public Dictionary<string, List<string>> Groupmap;
        public Dictionary<string, Dictionary<string, string>> MsgNotReadMap;//人，群
        public IMongoCollection<ChatLog> _chatlog;
        private IMongoCollection<ChatGroup> _groups;
        private IMongoCollection<Profile> profiles;
        public IMongoDatabase database;
        private ILogger<ChatService> logger;
        private Dictionary<string, (IDisposable, IDisposable)> SubscriptionMap;

        public ChatService(IDbSettings settings, ILogger<ChatService> logger)
        {

            var client = new MongoClient(settings.DbConnection);
            database = client.GetDatabase(settings.DbName);
            _chatlog = database.GetCollection<ChatLog>(settings.ChatLogCollectionName);
            _groups = database.GetCollection<ChatGroup>(settings.ChatGroupCollectionName);
            this.profiles = database.GetCollection<Profile>(settings.ProfileCollectionName);

            WsCastMap = new Dictionary<string, JsonWebsocketWrapper<WsRecvMsg, WsSendMsg>>();
            MsgNotReadMap = new Dictionary<string, Dictionary<string, string>>();
            SubscriptionMap = new Dictionary<string, (IDisposable, IDisposable)>();
            this.logger = logger;
        }

        public class UserAlreadyConnectedException : Exception { }

        public async Task<JsonWebsocketWrapper<WsRecvMsg, WsSendMsg>> Addsocket(String name, WebSocket webSocket, JsonSerializerOptions jsonSerializer)
        {
            var tgt = new JsonWebsocketWrapper<WsRecvMsg, WsSendMsg>(webSocket, jsonSerializer);
            //check
            if (WsCastMap.ContainsKey(name))
            {
                string message = $"WsCastMap already contains name {name}, please recheck!";
                logger.LogWarning(message);
                throw new UserAlreadyConnectedException();
            }
            logger.LogInformation($"Added {name} into WsCastMap");
            //insert
            WsCastMap.Add(name, tgt);
            //process unread
            {
                try
                {
                    var (allunread, unreadMsg) = await GetAllUnreadCountOfUser(name);
                    await tgt.SendMessage(new WsSendUnreadCountMsg() { items = unreadMsg });
                    foreach (var item in allunread)
                    {
                        await tgt.SendMessage(item);
                    }
                }
                catch (Exception e)
                {
                    this.SendErrorMessage(name, e);
                }
            }
            //remind friends
            RemindFriend(true, name);
            //subscribe
            var msgSub = tgt.Messages.Subscribe(
                (msg) => { DealMsg(name, msg); },
                (err) =>
                {
                    Console.WriteLine("err: {0}", err);
                    this.SendErrorMessage(name, err);
                    this.OnUserGoingOffline(name);
                },
                () => { this.OnUserGoingOffline(name); });
            var errSub = tgt.Errors.Subscribe((e) => { this.SendErrorMessage(name, e); });

            this.SubscriptionMap.Add(name, (msgSub, errSub));

            return tgt;
        }

        public async void OnUserGoingOffline(string username)
        {
            WsCastMap.Remove(username);
            this.SubscriptionMap.Remove(username);

            logger.LogInformation($"User {username} goes offline");
        }
        public async void RemindFriend(bool state, string username)
        {
            var user = await profiles.AsQueryable().Where(o => username == o.Username).FirstOrDefaultAsync();
            if (user == null)
                return;
            var online = new WsSendOnlineStatusMsg()
            {
                userId = username,
                online = state
            };
            foreach (var friend in user.Friends)
            {
                if (WsCastMap.ContainsKey(friend))
                {
                    await WsCastMap[friend].SendMessage(online);
                }
            }
        }
        public async void SendErrorMessage(string username, Exception error, WsSendMsg sourceMessage = null)
        {
            try
            {
                await this.WsCastMap[username].SendMessage(new WsSendErrMsg()
                {
                    replyTo = sourceMessage?.id,
                    error = error.ToString(),
                    id = ObjectId.GenerateNewId().ToString(),
                });
            }
            catch (WebSocketException) { this.OnUserGoingOffline(username); }
        }

        public async void DealMsg(string name, WsRecvMsg recv)
        {
            try
            {
                if (recv is WsRecvChatMsg msg0)
                {
                    await DealMsg(name, msg0);
                }
                else if (recv is WsRecvReadPositionMsg msg1)
                {
                    await ProcessReadPositionMessage(name, msg1);
                }

            }
            catch (Exception e)
            {
                this.logger.LogWarning(new EventId(), e, "Error when processing message");
                await this.WsCastMap[name].SendMessage(new WsSendErrMsg()
                {
                    replyTo = recv.id,
                    error = e.ToString(),
                    id = ObjectId.GenerateNewId().ToString(),
                });
            }
        }

        public async Task DealMsg(string name, WsRecvChatMsg recv)
        {
            //Console.WriteLine(recv.msg.GetType());

            var groupname = recv.chatId;
            var group = await FindGroup(groupname);

            var members = group.members;
            var msg = await ToSendWsMsg(name, recv);
            InsertChat(groupname, msg);
            foreach (var men in members)
            {
                if (WsCastMap.ContainsKey(men))
                {
                    await WsCastMap[men].SendMessage(msg);
                }

            }
        }

        public async Task<WsSendChatMsg> ToSendWsMsg(string name, WsRecvChatMsg recvMsg)
        {
            SendMessage tgt = null;
            var swsmsg = new WsSendChatMsg();
            if (recvMsg.msg.GetType() == typeof(RTextMsg))
                tgt = ToSendMsg(name, recvMsg.msg as RTextMsg);
            else if (recvMsg.msg.GetType() == typeof(RFileMsg))
                tgt = ToSendMsg(name, recvMsg.msg as RFileMsg);
            else if (recvMsg.msg.GetType() == typeof(RImageMsg))
                tgt = ToSendMsg(name, recvMsg.msg as RImageMsg);
            else if (recvMsg.msg.GetType() == typeof(RForwardMsg))
            {
                var forwardmsg = await getMsg((recvMsg.msg as RForwardMsg).fromChatId, (recvMsg.msg as RForwardMsg).fromMessageId);
                forwardmsg.replyTo = null;
                tgt = forwardmsg.msg;
                tgt.id = ObjectId.GenerateNewId().ToString();
                tgt.time = DateTime.Now;
                tgt.forwardFrom = new FowardProperty()
                {
                    username = forwardmsg.msg.sender,
                    chatId = (recvMsg.msg as RForwardMsg).fromChatId,
                    msgId = (recvMsg.msg as RForwardMsg).fromMessageId
                };
                tgt.sender = name;

            }
            if (recvMsg.replyTo != null)
            {
                var reply = await getMsg(recvMsg.chatId, recvMsg.replyTo);
                tgt.replyTo = reply.msg;
            }


            swsmsg.chatId = recvMsg.chatId;
            swsmsg.msg = tgt;


            return swsmsg;
        }
        public SendMessage ToSendMsg(string name, RecvMessage tgt)
        {
            return new SendMessage();
        }
        public TextMsg ToSendMsg(string name, RTextMsg tgt)
        {

            var msg = new TextMsg()
            {
                id = ObjectId.GenerateNewId().ToString(),
                sender = name,
                time = DateTime.Now,
                text = tgt.text

            };

            return msg;
        }
        public FileMsg ToSendMsg(string name, RFileMsg tgt)
        {
            var msg = new FileMsg()
            {
                id = ObjectId.GenerateNewId().ToString(),
                sender = name,
                time = DateTime.Now,
                file = tgt.file,
                filename = tgt.filename,
                caption = tgt.caption,
                size = tgt.size

            };

            return msg;
        }
        public ImageMsg ToSendMsg(string name, RImageMsg tgt)
        {
            var msg = new ImageMsg()
            {
                id = ObjectId.GenerateNewId().ToString(),
                sender = name,
                time = DateTime.Now,
                image = tgt.Image,
                caption = tgt.Caption
            };
            return msg;
        }

        public async void SendAll(List<JsonWebsocketWrapper<WsRecvChatMsg, WsSendChatMsg>> clo, WsSendChatMsg Message)
        {
            foreach (var item in clo)
            {
                await item.SendMessage(Message);
            }
        }





        //unread first version
        private async Task ProcessReadPositionMessage(string username, WsRecvReadPositionMsg msg)
        {
            var (count, id) = await UpdateAndCountUnreadMessage(msg.chatId, username, new ObjectId(msg.msgId));
            if (this.WsCastMap.TryGetValue(username, out var websocketWrapper))
            {
                await websocketWrapper.SendMessage(new WsSendUnreadCountMsg()
                {
                    id = ObjectId.GenerateNewId().ToString(),
                    replyTo = msg.id,
                    items = new Dictionary<string, UnreadProperty>()
                    {
                        [msg.chatId] = new UnreadProperty() { count = (int)count, maxMessage = id }
                    }
                });
            }
        }

        private async Task<(long, ObjectId)> UpdateAndCountUnreadMessage(string groupId, string userId, ObjectId messageId)
        {
            var group = await this._groups.FindOneAndUpdateAsync(
                new FilterDefinitionBuilder<ChatGroup>().Where(group => group.name == groupId),
                new UpdateDefinitionBuilder<ChatGroup>().Combine(new[]{
                    new UpdateDefinitionBuilder<ChatGroup>().Set((group) => group.UserLatestRead[userId], messageId)
                }),
                new FindOneAndUpdateOptions<ChatGroup, ChatGroup>() { ReturnDocument = ReturnDocument.After }
            );

            ObjectId lastMessage = group.UserLatestRead[userId];

            var count = await this.database.GetCollection<WsSendChatMsg>(groupId).CountDocumentsAsync(
                new FilterDefinitionBuilder<WsSendChatMsg>().Where(msg => msg.id.CompareTo(lastMessage.ToString()) > 0)
            );

            return (count, lastMessage);
        }

        private async Task<(List<WsSendChatMsg>, Dictionary<string, UnreadProperty>)> GetAllUnreadCountOfUser(string userId)
        {
            var user = await this.profiles.AsQueryable().SingleAsync(p => p.Username == userId);
            var res = new Dictionary<string, UnreadProperty>();
            var unreadmsgs = new List<WsSendChatMsg>();
            // TODO: Silly O(n) client side trick. Is it possible to perform totally inside server?
            // (possibly not.)
            foreach (var g in user.Groups)
            {
                var group = await this._groups.AsQueryable().Where(group => group.name == g).SingleAsync();
                if (!group.UserLatestRead.TryGetValue(userId, out var lastMessage))
                {
                    await this._groups.UpdateOneAsync(g => g.id == group.id, Builders<ChatGroup>.Update.Set($"UserLastestRead.{user}", ObjectId.Empty));
                    lastMessage = ObjectId.Empty;
                }
                var groupLogCollection = this.database.GetCollection<WsSendChatMsg>(g);
                var count = await groupLogCollection.CountDocumentsAsync(
                    new FilterDefinitionBuilder<WsSendChatMsg>().Where(msg => msg.msg.id.CompareTo(lastMessage.ToString()) > 0)
                );
                var msgs = await groupLogCollection.AsQueryable().Where(msgs => msgs.msg.id.CompareTo(lastMessage.ToString()) > 0).ToListAsync();
                unreadmsgs.AddRange(msgs);
                res.Add(g, new UnreadProperty() { count = (int)count, maxMessage = lastMessage });
            }
            foreach (var f in user.Friends)
            {
                string g = null;
                if (String.Compare(user.Username, f) > 0)
                {
                    g = user.Username + "+" + f;
                }
                else if (String.Compare(user.Username, f) < 0)
                {
                    g = f + "+" + user.Username;
                }
                var group = await this._groups.AsQueryable().Where(group => group.name == g).SingleOrDefaultAsync();
                if (group == null) continue;
                ObjectId lastMessage = group.UserLatestRead[userId];
                var groupLogCollection = this.database.GetCollection<WsSendChatMsg>(g);
                var count = await groupLogCollection.CountDocumentsAsync(
                    new FilterDefinitionBuilder<WsSendChatMsg>().Where(msg => msg.msg.id.CompareTo(lastMessage.ToString()) > 0)
                );
                var msgs = await groupLogCollection.AsQueryable().Where(msgs => msgs.msg.id.CompareTo(lastMessage.ToString()) > 0).ToListAsync();
                unreadmsgs.AddRange(msgs);
                res.Add(g, new UnreadProperty() { count = (int)count, maxMessage = lastMessage });
            }

            return (unreadmsgs, res);
        }

        public async Task<List<ChatListItem>> GetChatlistItems(string userId)
        {
            var user = await this.profiles.AsQueryable().SingleAsync(p => p.Username == userId);

            var list = new List<ChatListItem>();

            // TODO: Silly O(n) client side trick. Is it possible to perform totally inside server?
            // (possibly not.)
            foreach (var g in user.Groups)
            {
                var group = await this._groups.AsQueryable().Where(group => group.name == g).SingleAsync();
                if (!group.UserLatestRead.TryGetValue(userId, out var lastUnread))
                {
                    await this._groups.UpdateOneAsync(g => g.id == group.id, Builders<ChatGroup>.Update.Set($"UserLastestRead.{user}", ObjectId.Empty));
                    lastUnread = ObjectId.Empty;
                }
                var groupLogCollection = this.database.GetCollection<WsSendChatMsg>(g);
                var count = await groupLogCollection.CountDocumentsAsync(
                    new FilterDefinitionBuilder<WsSendChatMsg>().Where(msg => msg.msg.id.CompareTo(lastUnread.ToString()) > 0)
                );
                var lastMessage = await groupLogCollection.AsQueryable().OrderByDescending(x => x.id).FirstOrDefaultAsync();
                list.Add(new ChatListItem()
                {
                    group = group,
                    unreadCount = (int)count,
                    message = lastMessage?.msg
                });
            }
            foreach (var f in user.Friends)
            {
                string g = null;
                if (String.Compare(user.Username, f) > 0)
                {
                    g = user.Username + "+" + f;
                }
                else if (String.Compare(user.Username, f) < 0)
                {
                    g = f + "+" + user.Username;
                }
                var group = await this._groups.AsQueryable().Where(group => group.name == g).SingleOrDefaultAsync();
                if (group == null) continue;
                ObjectId lastUnreadMessage = group.UserLatestRead[userId];
                var groupLogCollection = this.database.GetCollection<WsSendChatMsg>(g);
                var count = await groupLogCollection.CountDocumentsAsync(
                    new FilterDefinitionBuilder<WsSendChatMsg>().Where(msg => msg.msg.id.CompareTo(lastUnreadMessage.ToString()) > 0)
                );
                var lastMessage = await groupLogCollection.AsQueryable().OrderByDescending(x => x.id).FirstOrDefaultAsync();
                list.Add(new ChatListItem()
                {
                    group = group,
                    unreadCount = (int)count,
                    message = lastMessage?.msg
                });
            }

            return list;
        }

        //database process
        public async Task<ChatGroup> FindGroup(string groupname)
        {
            var tgt = await _groups.AsQueryable().Where(o => o.name == groupname).FirstOrDefaultAsync();
            return tgt;
        }

        public async void InsertChat(string groupname, WsSendChatMsg sendMsg)
        {
            IMongoCollection<WsSendMsg> groupLogCollection = database.GetCollection<WsSendMsg>(groupname);
            await groupLogCollection.InsertOneAsync(sendMsg);

        }
        public async Task<List<WsSendChatMsg>> getGroupMsg(string basemsg, int take, string groupname, bool reverse)
        {

            var groupLogCollection = database.GetCollection<WsSendChatMsg>(groupname);
            if (!reverse)
            {
                var msgs = await groupLogCollection.AsQueryable().Where(o => o.msg.id.CompareTo(basemsg) > 0).OrderBy(o => o.msg.id).Take(take).ToListAsync();
                return msgs;
            }
            else
            {
                var msgs = await groupLogCollection.AsQueryable().Where(o => o.msg.id.CompareTo(basemsg) < 0).OrderBy(o => o.msg.id).Take(take).ToListAsync();
                return msgs;
            }
        }
        public async Task<List<WsSendChatMsg>> getGroupMsg(string groupname)
        {

            var groupLogCollection = database.GetCollection<WsSendChatMsg>(groupname);
            var msgs = await groupLogCollection.AsQueryable().OrderBy(o => o.msg.id).ToListAsync();
            return msgs;
        }
        public async Task<WsSendChatMsg> getMsg(string groupname, string msgid)
        {

            var groupLogCollection = database.GetCollection<WsSendChatMsg>(groupname);
            var msg = await groupLogCollection.AsQueryable().Where(o => o.msg.id == msgid).FirstOrDefaultAsync();
            return msg;

        }

    }
}
