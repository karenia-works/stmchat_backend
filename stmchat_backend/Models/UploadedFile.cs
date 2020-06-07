using System.Collections.Generic;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace stmchat_backend.Models
{
    public class UploadedFile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string FileName { get; set; }

        public string FileFormat { get; set; }
        public bool IsImage { get; set; }

        public UploadedFile(string filename)
        {
            FileName = filename;
            FileFormat = Path.GetExtension(filename);
            IsImage = _imageFormats.Contains(FileFormat.ToLower());
        }

        public string GetUri()
        {
            return string.Join('/', @"/file", Id);
        }

        // From mime.types from nginx
        private static readonly List<string> _imageFormats = new List<string>
        {
            "jpg", "jpeg", "jfif", "png", "gif", "webp", "bmp", "jng", "svg", "svgz", "tif", "tiff", "wbmp", "ico"
        };
    }
}