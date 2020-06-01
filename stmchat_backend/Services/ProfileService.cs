using System;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using stmchat_backend.Models;
using stmchat_backend.Models.Settings;

namespace stmchat_backend.Services
{
    //针对用户信息的增删改查，暂不考虑聊天相关
    class ProfileService
    {
        private readonly IMongoCollection<Profile> _profile;
        private readonly IDbSettings _settings;
        private IMongoDatabase _database;

        public ProfileService(IDbSettings settings)
        {
            _settings = settings;
            var client = new MongoClient(settings.DbConnection);
            _database = client.GetDatabase(settings.DbName);
            _profile = _database.GetCollection<Profile>(settings.ProfileCollectionName);
        }

        public async Task<List<Profile>> GetProfileList()
        {
            return await _profile
                .AsQueryable()
                .OrderBy(p => p.Username)
                .ToListAsync();
        }

        public async Task<Profile> GetProfileByUsername(string username)
        {
            return await _profile
                .AsQueryable()
                .Where(p => p.Username == username)
                .FirstOrDefaultAsync();
        }

        // TODO: more actions
    }
}