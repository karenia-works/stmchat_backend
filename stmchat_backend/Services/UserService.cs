using MongoDB.Driver;
using MongoDB.Bson;
using stmchat_backend.Models;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using stmchat_backend.Models.Settings;
using System.Threading.Tasks;


//同Userservice
namespace stmchat_backend.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        public UserService(IDbSettings settings)
        {
            var client = new MongoClient(settings.DbConnection);
            var database = client.GetDatabase(settings.DbName);
            _users = database.GetCollection<User>(settings.UserCollectionName);
        }
        public async Task<User> findUser(string username)
        {
            var query = await _users.AsQueryable().Where(o => o.Username == username).FirstOrDefaultAsync();
            return query;
        }
        public async Task<DeleteResult> deleteUser(string username)
        {
            var result = await _users.DeleteOneAsync(o => o.Username == username);
            return result;
        }
        public async Task<UpdateResult> updateUser(User user)
        {
            var flicker = Builders<User>.Filter.Eq("id", user.Id);
            var update = Builders<User>.Update.Set("Username", user.Username).Set("Password", user.Password);
            var result = await _users.UpdateOneAsync(flicker, update);
            return result;
        }
    }
}