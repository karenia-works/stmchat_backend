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


namespace stmchat_backend.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    class ProfileController : ControllerBase
    {
        private ProfileService _service;

        public ProfileController(ProfileService service)
        {
            _service = service;
        }

        // 可能会因为用户名叫`me`而出错
        [HttpGet("me")]
        public async Task<IActionResult> Get()
        {
            // TODO: impl it after id4 set
            return Ok(_service.GetProfileList());
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> Get(string username)
        {
            var res = await _service.GetProfileByUsername(username);
            if (res == null)
            {
                return NotFound();
            }

            return Ok(res);
        }
    }
}