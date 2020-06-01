using System;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using stmchat_backend.Models;
using stmchat_backend.Services;


namespace stmchat_backend.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    class ProfileController : ControllerBase
    {
        private ProfileService _service;

        public ProfileController(ProfileService service)
        {
            _service = service;
        }

        [HttpGet("me")]
        public async Task<Profile> Get()
        {
            // TODO: impl it
            return null;
        }
        
        [HttpGet("{username}")]
        public async Task<Profile> Get(string username)
        {
            // TODO: impl it
            return null;
        }
    }
}