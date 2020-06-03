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
        public UserService _userservice { get; set; }

        public UserStore(UserService userService)
        {
            _userservice = userService;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var result = await _userservice.findUser(context.UserName);
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
                    subject: result.Id,
                    authenticationMethod: "custom",
                    claims: new Claim[]
                    {
                        new Claim("id", result.Id),
                        new Claim("Name", result.Username),
                    }
                );
            }
        }
    }
}