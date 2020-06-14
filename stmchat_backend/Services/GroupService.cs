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

        public GroupService(IDbSettings settings)
        {
            var client = new MongoClient(settings.DbConnection);
            var database = client.GetDatabase(settings.DbName);
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

            var chatlogid = ObjectId.GenerateNewId().ToString();
            creating.chatlog = chatlogid;
            await _groups.InsertOneAsync(creating);
            var chatlogmake = new ChatLog()
            {
                id = chatlogid
            };
            await _chatlogs.InsertOneAsync(chatlogmake);
            return creating;
        }

        public async Task<ChatGroup> FindGroup(string groupName)
        {
            return await _groups
                .AsQueryable()
                .Where(o => o.name == groupName)
                .FirstOrDefaultAsync();
        }

        public async Task<UpdateResult> AddGroup(string user, string groupName)
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

            var flicker = Builders<ChatGroup>.Filter.Eq(o => o.name, groupName);
            var update = Builders<ChatGroup>.Update.Push(o => o.members, user);

            return await _groups.UpdateOneAsync(flicker, update);
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

            await _chatlogs.DeleteOneAsync(o => o.id == group.chatlog);
            return await _groups.DeleteOneAsync(o => o.name == groupName);
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
                    isFriend = true

                };

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

                chat.members.Add(owner);
                chat.members.Add(passby);
                await MakeGroup(chat);
            }
            return "ok";
        }
    }
}