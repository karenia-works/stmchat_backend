using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using stmchat_backend.Models;
using stmchat_backend.Models.Settings;

namespace stmchat_backend.Services
{
    public class FileService
    {
        private readonly IMongoCollection<UploadedFile> _file;

        public FileService(IDbSettings settings)
        {
            var client = new MongoClient(settings.DbConnection);
            var database = client.GetDatabase(settings.DbName);
            _file = database.GetCollection<UploadedFile>(settings.FileBucketName);
        }

        public async Task<UploadedFile> GetFileInfo(string fileId)
        {
            var res = await _file
                .AsQueryable()
                .Where(f => f.Id == fileId)
                .FirstOrDefaultAsync();
            return res;
        }

        public async void SaveInfo(string filename)
        {
            var res = await GetFileInfo(filename);
            if (res == null)
            {
                await _file.InsertOneAsync(new UploadedFile(filename));
            }
        }
    }
}