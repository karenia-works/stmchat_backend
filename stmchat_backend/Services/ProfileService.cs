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
    public class ProfileService
    {
        private readonly IMongoCollection<Profile> _profile;
        private readonly IMongoCollection<ChatGroup> _group;

        public ProfileService(IDbSettings settings)
        {
            var client = new MongoClient(settings.DbConnection);
            var database = client.GetDatabase(settings.DbName);
            _profile = database.GetCollection<Profile>(settings.ProfileCollectionName);
            _group = database.GetCollection<ChatGroup>(settings.ChatGroupCollectionName);
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

        public async Task<Profile> GetProfileById(string id)
        {
            return await _profile
                .AsQueryable()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Profile>> GetUserFriends(string username)
        {
            var profile = await GetProfileByUsername(username);

            var res = profile?
                .Friends
                .Select(f => GetProfileByUsername(f).Result)
                .ToList();
            return res;
        }

        public async Task<List<ChatGroup>> GetUserGroups(string username)
        {
            var profile = await GetProfileByUsername(username);

            var res = profile?.Groups
                .Select(i =>
                    _group.AsQueryable()
                        .Where(cg => cg.id == i)
                        .FirstOrDefaultAsync()
                        .Result
                )
                .ToList();
            return res;
        }

        public async Task<Profile> CreateProfile(Profile profile)
        {
            var profileResult = await GetProfileByUsername(profile.Username);
            if (profileResult != null)
            {
                return null;
            }

            await _profile.InsertOneAsync(profile);
            return await GetProfileByUsername(profile.Username);
        }

        public async Task<UpdateResult> AddUserFriend(string username, string friendname)
        {
            var profile = await GetProfileByUsername(username);
            profile.Friends.Add(friendname);
            var flicker = Builders<Profile>.Filter.Eq("Username", username);
            var update = Builders<Profile>
                .Update.Set("Friends", profile.Friends);
            var result = await _profile.UpdateOneAsync(flicker, update);
            return result;
        }
        public async Task<UpdateResult> AddUserGroup(string username, string groupname)
        {
            var profile = await GetProfileByUsername(username);
            profile.Groups.Add(groupname);
            var flicker = Builders<Profile>.Filter.Eq("Username", username);
            var update = Builders<Profile>
                .Update.Set("Groups", profile.Groups);
            var result = await _profile.UpdateOneAsync(flicker, update);
            return result;
        }
        public async Task<UpdateResult> DeleteUserFriend(string username, string friendname)
        {
            var profile = await GetProfileByUsername(username);
            profile.Friends.Remove(friendname);
            var flicker = Builders<Profile>.Filter.Eq("Username", username);
            var update = Builders<Profile>
                .Update.Set("Friends", profile.Friends);
            var result = await _profile.UpdateOneAsync(flicker, update);
            return result;
        }

        public async Task<bool> isFriend(string username, string friendname)
        {
            var profile = await GetProfileByUsername(username);
            if (profile.Friends.Contains(friendname))
                return true;
            else
                return false;
        }
    }
}