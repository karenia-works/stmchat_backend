using IdentityServer4.Validation;
using System.Threading.Tasks;
using System.Security.Claims;
using stmchat_backend.Models;
using stmchat_backend.Services;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4;
using System.Collections.Generic;

namespace stmchat_backend.Store
{
    public class UserStore : IResourceOwnerPasswordValidator
    {
        private readonly UserService _userService;

        public UserStore(UserService userService)
        {
            _userService = userService;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var result = await _userService.findUser(context.UserName);
            if (result == null || !result.CheckPassword(context.Password))
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    "Username and password do not match");
                return;
            }
            else
            {
                context.Result = new GrantValidationResult(
                    result.Id,
                    "custom",
                    new Claim[]
                    {
                        new Claim("id", result.Id),
                        new Claim("Name", result.Username),
                    }
                );
            }
        }
    }
}