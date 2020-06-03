using MongoDB.Driver;
using MongoDB.Bson;
using stmchat_backend.Models;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using stmchat_backend.Models.Settings;
using System.Threading.Tasks;
using stmchat_backend.Models;

//同Userservice
namespace stmchat_backend.Services
{
    public class UserService
    {
        public async Task<User> findUser(string username)
        {
            return null;
        }
    }
}