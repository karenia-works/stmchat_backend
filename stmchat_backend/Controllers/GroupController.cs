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
    [Route("api/v1/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> FindGroup(string name)
        {
            // YOUR API IS A HALL OF SHAME
            // YOU GIVE REST A BAD NAME
            // YOU SAID IT WORKS IN A RESTFUL WAY
            // THEN YOUR ERRORS COME BACK AS 200 OK
            var res = await groupservice.FindGroup(name);
            if (res != null) return Ok(res);
            else return NotFound();
        }

        [Authorize("user")]
        [HttpPost]
        public async Task<IActionResult> MakeGroup([FromBody] ChatGroup tgt)
        {
            var user = User.Claims.Where(Clame => Clame.Type == "Name").FirstOrDefault().Value;
            tgt.owner = user;
            tgt.UserLatestRead = new Dictionary<string, ObjectId>();
            tgt.UserLatestRead.Add(user, ObjectId.GenerateNewId());
            await groupservice.MakeGroup(tgt);
            return Ok(tgt);
        }

        [HttpPut("{user}/add/{name}")]
        public async Task<IActionResult> AddGroup(string user, string name)
        {
            //var user = User.Claims.Where(Clame => Clame.Type == "Name").FirstOrDefault().Value;
            await profileservice.AddUserGroup(user, name);
            await groupservice.AddGroup(name, user);
            return Ok();
        }

        // //[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
        // [HttpPost("{user}/makefriend/{name}")]
        // public async Task<string> MakeFriend(string user, string name)
        // {
        //     // var user = User.Claims.Where(Clame => Clame.Type == "Name").FirstOrDefault().Value;

        //     var chat = new ChatGroup()
        //     {
        //         id = ObjectId.GenerateNewId().ToString(),
        //         name = name + user,
        //         owner = user,
        //         isFriend = false
        //     };
        //     chat.members.Add(name);
        //     chat.members.Add(user);
        //     await groupservice.MakeGroup(chat);
        //     return "ok";
        // }

        //test
    }
}
