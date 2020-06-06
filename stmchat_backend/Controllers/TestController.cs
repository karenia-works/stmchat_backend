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

        public static List<JsonWebsocketWrapper<string, string>> testsockets = new List<JsonWebsocketWrapper<string, string>>();
        public static async void Addsocket(WebSocket webSocket)
        {
            Console.WriteLine("make");
            var tgt = new JsonWebsocketWrapper<string, string>(webSocket);
            testsockets.Add(tgt);
            await tgt.Messages.Subscribe()



        }
        [HttpGet("test")]

        public async Task<string> jsontest()
        {

            foreach (var socket in testsockets)
            {
                await socket.SendMessage("fuck");
            }
            return "ok";
        }

        [HttpGet]
        public string hello()
        {
            return "hello";
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