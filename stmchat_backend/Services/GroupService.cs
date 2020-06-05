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
        private IMongoCollection<ChatGroup> _groups { get; set; }
        private IMongoCollection<ChatLog> _chatlogs { get; set; }
        public GroupService(IDbSettings settings)
        {

            var client = new MongoClient(settings.DbConnection);
            var _database = client.GetDatabase(settings.DbName);
            _groups = _database.GetCollection<ChatGroup>(settings.ChatGroupCollectionName);
            _chatlogs = _database.GetCollection<ChatLog>(settings.ChatLogCollectionName);
        }
        public async Task<ChatGroup> MakeGroup(string ChatlogId, ChatGroup creating)
        {
            creating.chatlog = ChatlogId;
            var tmp = await _groups.AsQueryable().Where(o => o.name == creating.name).FirstOrDefaultAsync();
            if (tmp != null)
            {
                return null;
            }
            await _groups.InsertOneAsync(creating);
            var res = await _groups.AsQueryable().Where(o => o.name == creating.name).FirstOrDefaultAsync();
            return res;
        }
        public async Task<ChatGroup> FindGroup(string groupname)
        {
            return await _groups.AsQueryable().Where(o => o.name == groupname).FirstOrDefaultAsync();
        }
        public async Task<UpdateResult> AddGroup(string groupname, string user)
        {
            var group = await _groups.AsQueryable().Where(o => o.name == groupname).FirstOrDefaultAsync();
            if (group == null)
                return null;
            if (group.members.Contains(user))
            {
                return null;
            }

            var flicker = Builders<ChatGroup>.Filter.Eq("name", groupname);
            var update = Builders<ChatGroup>.Update.Push(o => o.members, user);

            return await _groups.UpdateOneAsync(flicker, update); ;
        }
        public async Task<DeleteResult> DeleteGroup(string groupname)
        {
            var group = await _groups.AsQueryable().Where(o => o.name == groupname).FirstOrDefaultAsync();
            if (group == null)
            {
                return null;
            }
            await _chatlogs.DeleteOneAsync(o => o.id == group.chatlog);
            return await _groups.DeleteOneAsync(o => o.name == groupname);
        }
    }
}