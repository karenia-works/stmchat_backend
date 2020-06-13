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
        public TestController(ChatService _chatservice, ProfileService _profileservice, GroupService _groupservice)
        {
            chatservice = _chatservice;
            profileservice = _profileservice;
            groupservice = _groupservice;
        }



        [HttpGet("insert")]
        public async Task<String> insert()
        {
            var ms_wang = new Profile()
            {
                Username = "wang"
            };
            ms_wang.Groups.Add("kruodis");
            var ms_yang = new Profile()
            {
                Username = "yang"
            };
            var ms_li = new Profile()
            {
                Username = "li"
            };
            var ms_he = new Profile()
            {
                Username = "he"
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
                name = "kruodis",
                isFriend = false,
                owner = "wang",
                describ = "wei are family",
                members = new List<string>()
            };
            group1.members.Add("wang");
            await groupservice.MakeGroup(group1);
            await profileservice.AddUserGroup("he", "kruodis");
            await groupservice.AddGroup("kurodis", "he");
            await profileservice.AddUserGroup("li", "kruodis");
            await groupservice.AddGroup("kurodis", "li");
            return "Ok";
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
    public class basecase
    {

    }
    [JsonDiscriminator("second")]
    public class secondcase : basecase
    {
        public int i { get; set; }
    }
}