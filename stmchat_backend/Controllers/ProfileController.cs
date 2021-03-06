using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using stmchat_backend.Models;
using stmchat_backend.Services;


namespace stmchat_backend.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _service;
        private GroupService _groupService;
        private UserService _userService;
        private ChatService _chatService;

        public ProfileController(ProfileService service, GroupService groupService, UserService userService, ChatService chatService)
        {
            _service = service;
            _groupService = groupService;
            _userService = userService;
            _chatService = chatService;
        }

        // 可能会因为用户名叫`me`而出错
        // 因此限制用户名至少为3位
        [Authorize("user")]
        [HttpGet("me")]

        public async Task<IActionResult> Get()
        {
            var profilename = User
                .Claims
                .Where(claim => claim.Type == "Name")
                .FirstOrDefault()
                .Value;
            var profile = await _service.GetProfileByUsername(profilename);
            if (_chatService.WsCastMap.ContainsKey(profilename))
                profile.state = true;
            else
                profile.state = false;
            return Ok(profile);
        }

        [HttpGet("test/{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var res = await _service.GetProfileByUsername(username);
            if (res == null)
            {
                return NotFound();
            }

            if (_chatService.WsCastMap.ContainsKey(username))
                res.state = true;
            else
                res.state = false;
            return Ok(res);
        }

        [HttpGet("{username}/friends")]
        public async Task<IActionResult> GetFriendsByUser(string username)
        {
            var res = await _service.GetUserFriends(username);
            if (res == null)
            {
                return NotFound();
            }

            return Ok(res);
        }

        [HttpGet("{username}/groups")]
        public async Task<IActionResult> GetGroupsByUser(string username)
        {
            var res = await _service.GetUserGroups(username);
            if (res == null)
            {
                return NotFound();
            }

            return Ok(res);
        }
        [HttpPost("register")]
        public async Task<IActionResult> register([FromBody] User user)
        {
            var res = await _userService.InsertUser(user);
            if (res == null)
            {
                BadRequest();
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Profile profile)
        {
            var res = await _service.CreateProfile(profile);
            if (res == null)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPatch("{username}")]
        public async Task<IActionResult> Patch(string username,
            [FromBody] JsonPatchDocument<Profile> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var profile = await _service.GetProfileByUsername(username);
            if (profile == null)
            {
                return NotFound();
            }

            patchDocument.ApplyTo(profile, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(profile);
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> Put(string username, [FromBody] Profile profile)
        {
            var profileResult = await _service.GetProfileByUsername(username);
            if (profileResult == null)
                return NotFound();
            if (profile == null || profileResult.Id != profile.Id)
                return BadRequest();
            var res = await _service.EditProfile(username, profile);
            if (res == null)
                return BadRequest();
            return Ok(res);
        }

        [HttpPost("{username}/friends/{friendname}")]
        public async Task<IActionResult> addFriend(string username, string friendname)
        {
            //var res = await _service.GetProfileByUsername(username);
            if (await _service.GetProfileByUsername(username) == null)
                return NotFound("user not found");

            if (await _service.GetProfileByUsername(friendname) == null)
                return NotFound("friend not found");

            if (await _service.isFriend(username, friendname))
                return BadRequest("already friend");

            var tem = await _service.AddUserFriend(username, friendname);

            if (tem == null)
                return BadRequest("add friend error");
            await _groupService.MakeFriend(username, friendname);
            return Ok();
        }

        [HttpDelete("{username}/friends/{friendname}")]
        public async Task<IActionResult> deleteFriend(string username, string friendname)
        {
            //var res = await _service.GetProfileByUsername(username);
            if (await _service.GetProfileByUsername(username) == null)
                return NotFound("user not found");

            if (await _service.GetProfileByUsername(friendname) == null)
                return NotFound("friend not found");

            if (!await _service.isFriend(username, friendname))
                return BadRequest("already not friend");

            var tem = await _service.DeleteUserFriend(username, friendname);
            await _groupService.DeleteFriend(username, friendname);
            if (tem == null)
                return BadRequest("delete friend error");
            return Ok();
        }
    }
}
