using MongoDB.Driver;
using MongoDB.Bson;
using stmchat_backend.Models;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using stmchat_backend.Models.Settings;
using System.Threading.Tasks;

namespace stmchat_backend.Services
{
    public class GroupService
    {
        private readonly IMongoCollection<ChatGroup> _groups;
        private readonly IMongoCollection<ChatLog> _chatlogs;

        public GroupService(IDbSettings settings)
        {
            var client = new MongoClient(settings.DbConnection);
            var database = client.GetDatabase(settings.DbName);
            _groups = database.GetCollection<ChatGroup>(settings.ChatGroupCollectionName);
            _chatlogs = database.GetCollection<ChatLog>(settings.ChatLogCollectionName);
        }

        public async Task<ChatGroup> MakeGroup(string chatlogId, ChatGroup creating)
        {
            creating.Chatlog = chatlogId;
            var tmp = await _groups
                .AsQueryable()
                .Where(o => o.GroupName == creating.GroupName)
                .FirstOrDefaultAsync();
            if (tmp != null)
            {
                return null;
            }

            await _groups.InsertOneAsync(creating);
            var res = await _groups
                .AsQueryable()
                .Where(o => o.GroupName == creating.GroupName)
                .FirstOrDefaultAsync();
            return res;
        }

        public async Task<ChatGroup> FindGroup(string groupName)
        {
            return await _groups
                .AsQueryable()
                .Where(o => o.GroupName == groupName)
                .FirstOrDefaultAsync();
        }

        public async Task<UpdateResult> AddGroup(string groupName, string user)
        {
            var group = await _groups
                .AsQueryable()
                .Where(o => o.GroupName == groupName)
                .FirstOrDefaultAsync();
            if (group == null)
                return null;
            if (group.Members.Contains(user))
            {
                return null;
            }

            var flicker = Builders<ChatGroup>.Filter.Eq("name", groupName);
            var update = Builders<ChatGroup>.Update.Push(o => o.Members, user);

            return await _groups.UpdateOneAsync(flicker, update);
        }

        public async Task<DeleteResult> DeleteGroup(string groupName)
        {
            var group = await _groups
                .AsQueryable()
                .Where(o => o.GroupName == groupName)
                .FirstOrDefaultAsync();
            if (group == null)
            {
                return null;
            }

            await _chatlogs.DeleteOneAsync(o => o.Id == group.Chatlog);
            return await _groups.DeleteOneAsync(o => o.GroupName == groupName);
        }
    }
}