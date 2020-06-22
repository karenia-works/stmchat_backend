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
    public class ChatController : ControllerBase
    {
        public ChatService _chatService;
        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }
        [HttpGet]
        public async Task<List<WsSendChatMsg>> getMsg([FromForm] string groupname, int skip = 0, int limit = 0)
        {
            if (skip == 0 && limit == 0)
            {
                return await _chatService.getGroupMsg(groupname);
            }
            else
                return await _chatService.getGroupMsg(skip, limit, groupname);
        }
    }
}
