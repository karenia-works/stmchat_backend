using MongoDB.Driver;
using MongoDB.Bson;
using stmchat_backend.Models;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using stmchat_backend.Models.Settings;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

//同Userservice
namespace stmchat_backend.Services
{
    public class UserService
    {
        public IMongoCollection<User> _users;

        public UserService(IDbSettings settings)
        {
            var client = new MongoClient(settings.DbConnection);
            var database = client.GetDatabase(settings.DbName);
            _users = database.GetCollection<User>(settings.UserCollectionName);
        }

        public async Task<User> FindUser(string username)
        {
            var query = await _users
                .AsQueryable()
                .Where(o => o.Username == username)
                .FirstOrDefaultAsync();
            return query;
        }

        public async Task<DeleteResult> DeleteUser(string username)
        {
            var result = await _users.DeleteOneAsync(o => o.Username == username);
            return result;
        }

        public async Task<UpdateResult> UpdateUser(User user)
        {
            var flicker = Builders<User>.Filter.Eq("id", user.Id);

            var update = Builders<User>
                .Update
                .Set("username", user.Username)
                .Set("password", user.Password);
            var result = await _users.UpdateOneAsync(flicker, update);
            return result;
        }
        public async Task<User> InsertUser(User user)
        {
            var tgt = await _users.AsQueryable().Where(o => o.Username == user.Username).FirstOrDefaultAsync();
            if (tgt != null)
                return null;
            await _users.InsertOneAsync(user);
            return user;

        }
    }
}