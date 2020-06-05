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
namespace stmchat_backend.Controllers
{
    [Route("/api/v1/test")]
    [ApiController]

    public class TestController : ControllerBase
    {
        [HttpGet("test")]

        public string jsontest()
        {

            var test = new TextMsg();
            test.id = "11";
            test.sender = "sss";
            test.text = "ddd";
            test.time = new DateTime();
            return "o~~k";
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