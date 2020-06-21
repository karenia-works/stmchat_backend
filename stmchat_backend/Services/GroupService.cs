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
        public IMongoCollection<ChatGroup> _groups;
        public IMongoCollection<ChatLog> _chatlogs;
        public IMongoDatabase database;
        public GroupService(IDbSettings settings)
        {
            var client = new MongoClient(settings.DbConnection);
            database = client.GetDatabase(settings.DbName);
            _groups = database.GetCollection<ChatGroup>(settings.ChatGroupCollectionName);
            _chatlogs = database.GetCollection<ChatLog>(settings.ChatLogCollectionName);
        }

        public async Task<ChatGroup> MakeGroup(ChatGroup creating)
        {

            var tmp = await _groups
                .AsQueryable()
                .Where(o => o.name == creating.name)
                .FirstOrDefaultAsync();
            if (tmp != null)
            {
                return null;
            }

            await _groups.InsertOneAsync(creating);
            return creating;
        }

        public async Task<ChatGroup> FindGroup(string groupName)
        {
            return await _groups
                .AsQueryable()
                .Where(o => o.name == groupName)
                .FirstOrDefaultAsync();
        }

        public async Task<ReplaceOneResult> AddGroup(string user, string groupName)
        {
            var group = await _groups
                .AsQueryable()
                .Where(o => o.name == groupName)
                .FirstOrDefaultAsync();
            if (group == null)
                return null;
            if (group.members.Contains(user))
            {
                return null;
            }
            group.UserLatestRead.Add(user, ObjectId.GenerateNewId());


            return await _groups.ReplaceOneAsync(o => o.name == groupName, group);
        }

        public async Task<DeleteResult> DeleteGroup(string groupName)
        {
            var group = await _groups
                .AsQueryable()
                .Where(o => o.name == groupName)
                .FirstOrDefaultAsync();
            if (group == null)
            {
                return null;
            }

            await database.DropCollectionAsync(groupName);
            return await _groups.DeleteOneAsync(o => o.name == groupName);
        }
        public async Task<DeleteResult> DeleteFriend(string username, string friendname)
        {
            string g = null;
            if (string.Compare(username, friendname) > 0)
            {
                g = username + "+" + friendname;
            }
            else
            {
                g = friendname + "+" + username;
            }

            await database.DropCollectionAsync(g);
            return await _groups.DeleteOneAsync(o => o.name == g);
        }
        public async Task<string> MakeFriend(string owner, string passby)
        {
            if (string.Compare(owner, passby) > 0)
            {
                var chat = new ChatGroup()
                {
                    id = ObjectId.GenerateNewId().ToString(),
                    name = owner + "+" + passby,
                    owner = owner,
                    members = new List<string>(),
                    isFriend = true,
                    UserLatestRead = new Dictionary<string, ObjectId>()
                };
                chat.UserLatestRead.Add(owner, ObjectId.GenerateNewId());
                chat.UserLatestRead.Add(passby, ObjectId.GenerateNewId());
                chat.members.Add(owner);
                chat.members.Add(passby);

                await MakeGroup(chat);
            }
            else
            {
                var chat = new ChatGroup()
                {
                    id = ObjectId.GenerateNewId().ToString(),
                    name = passby + "+" + owner,
                    owner = owner,
                    members = new List<string>()

                };
                chat.UserLatestRead.Add(owner, ObjectId.GenerateNewId());
                chat.UserLatestRead.Add(passby, ObjectId.GenerateNewId());
                chat.members.Add(owner);
                chat.members.Add(passby);
                await MakeGroup(chat);
            }
            return "ok";
        }
    }
}
