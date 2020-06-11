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
        public TestController(ChatService _chatservice)
        {
            chatservice = _chatservice;
        }
        [HttpGet("test")]

        public async Task<List<WsSendMsg>> jsontest()
        {

            var res = await chatservice.getGroupMsg("5ee10025d362711ed88b511a", 1);
            return res;
        }

        [HttpGet]
        public string hello()
        {
            return "hello";
        }
        [HttpGet("insert")]
        public String insert()
        {

            chatservice.insert();
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