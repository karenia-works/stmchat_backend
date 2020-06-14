using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using stmchat_backend.Models;
using stmchat_backend.Services;
using MongoDB.Bson;
using System.Collections.Generic;
namespace stmchat_backend.Controllers
{
    public class GroupController : ControllerBase
    {
        public GroupService groupservice;
        public ProfileService profileservice;
        public GroupController(GroupService _groupservice, ProfileService _profileservice)
        {
            groupservice = _groupservice;
            profileservice = _profileservice;
        }
        [HttpGet("{name}")]
        public async Task<ChatGroup> FindGroup(string name)
        {
            return await groupservice.FindGroup(name);
        }
        //[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
        [HttpPost]
        public async Task<string> MakeGroup([FromBody] ChatGroup tgt)
        {
            var user = User.Claims.Where(Clame => Clame.Type == "Name").FirstOrDefault();
            tgt.owner = user.Value;
            await groupservice.MakeGroup(tgt);
            return "ok";
        }
        [HttpPut("{user}/add/{name}")]
        public async Task<string> AddGroup(string user, string name)
        {
            //var user = User.Claims.Where(Clame => Clame.Type == "Name").FirstOrDefault().Value;
            await profileservice.AddUserGroup(user, name);
            await groupservice.AddGroup(name, user);
            return "ok";
        }
        //[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
        [HttpPost("{user}/makefriend/{name}")]
        public async Task<string> MakeFriend(string user, string name)
        {

            // var user = User.Claims.Where(Clame => Clame.Type == "Name").FirstOrDefault().Value;

            var chat = new ChatGroup()
            {
                id = ObjectId.GenerateNewId().ToString(),
                name = name + user,
                owner = user,
                isFriend = false

            };
            chat.members.Add(name);
            chat.members.Add(user);
            await groupservice.MakeGroup(chat);
            return "ok";
        }
        //test

    }
}