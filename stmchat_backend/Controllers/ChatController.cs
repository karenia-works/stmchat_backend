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
        [HttpGet("{groupName}?start={start_id}&limit={limit}&reverse={true|false}")]
        public async Task<List<WsSendChatMsg>> getMsg(string groupName, string start_id, int limit, bool reverse)
        {
            return await _chatService.getGroupMsg(start_id, limit, groupName, reverse);
        }
    }
}
