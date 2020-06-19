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
namespace stmchat_backend.Controllers
{
    [Route("/api/v1/test")]
    [ApiController]

    public class TestController : ControllerBase
    {

        public ChatService chatservice;
        public ProfileService profileservice;
        public GroupService groupservice;
        public UserService userservice;
        public TestController(ChatService _chatservice, ProfileService _profileservice, GroupService _groupservice, UserService _userservice)
        {
            chatservice = _chatservice;
            profileservice = _profileservice;
            groupservice = _groupservice;
            userservice = _userservice;
        }
        [HttpGet("msgexp")]
        public WsRecvMsg msgexp()
        {
            var tgt = new WsRecvMsg()
            {
                chatId = "family",
                msg = new RTextMsg()
                {
                    text = "fill"
                }
            };
            return tgt;
        }
        [HttpGet("dbtest")]
        public String dbtest()
        {
            return groupservice._groups.Count(Builders<ChatGroup>.Filter.Empty).ToString();
        }

        [HttpGet("insert")]
        public async Task<String> insert()
        {
            var ms_wang = new Profile()
            {
                Username = "wang",
                Groups = new List<string>(),
                Friends = new List<string>()
            };
            ms_wang.Groups.Add("kruodis");
            var ms_yang = new Profile()
            {
                Username = "yang",
                Groups = new List<string>(),
                Friends = new List<string>()
            };
            var ms_li = new Profile()
            {
                Username = "li",
                Groups = new List<string>(),
                Friends = new List<string>()
            };
            var ms_he = new Profile()
            {
                Username = "he",
                Groups = new List<string>(),
                Friends = new List<string>()
            };
            await profileservice.CreateProfile(ms_wang);
            await profileservice.CreateProfile(ms_yang);
            await profileservice.CreateProfile(ms_li);
            await profileservice.CreateProfile(ms_he);
            await profileservice.AddUserFriend("wang", "li");
            await groupservice.MakeFriend("wang", "li");
            await profileservice.AddUserFriend("wang", "yang");
            await groupservice.MakeFriend("wang", "yang");
            var group1 = new ChatGroup()
            {
                name = "family",
                isFriend = false,
                owner = "wang",
                describ = "wei are family",
                members = new List<string>()
            };
            group1.members.Add("wang");
            await groupservice.MakeGroup(group1);
            await profileservice.AddUserGroup("he", "family");
            await groupservice.AddGroup("he", "family");
            await profileservice.AddUserGroup("li", "family");
            await groupservice.AddGroup("li", "family");
            await userservice.InsertUser(new Models.User() { Username = "wang", Password = "wang" });
            await userservice.InsertUser(new Models.User() { Username = "yang", Password = "yang" });
            await userservice.InsertUser(new Models.User() { Username = "li", Password = "li" });
            await userservice.InsertUser(new Models.User() { Username = "he", Password = "he" });
            return "Ok";
        }
        [HttpDelete("killall")]
        public String killall()
        {
            groupservice._chatlogs.DeleteMany(Builders<ChatLog>.Filter.Empty);
            groupservice._groups.DeleteMany(Builders<ChatGroup>.Filter.Empty);
            profileservice._profile.DeleteMany(Builders<Profile>.Filter.Empty);
            return "hahahahaha";
        }
        [HttpGet("what")]
        public List<ChatLog> what()
        {
            var into = new List<ChatLog>();
            var res = new ChatLog() { id = "", messages = new List<WsSendMsg>() };
            var m1 = new WsSendMsg()
            {
                chatId = ObjectId.GenerateNewId().ToString(),
                msg = new TextMsg()
                {
                    sender = "wang",
                    time = DateTime.Now,
                    forwardFrom = "sssdd",
                    replyTo = ObjectId.GenerateNewId().ToString()
                }
            };
            res.messages.Add(m1);

            into.Add(res);
            into.Add(res);
            return into;

        }
    }


}