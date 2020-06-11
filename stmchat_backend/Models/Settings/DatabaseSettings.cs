namespace stmchat_backend.Models.Settings
{
    public class DbSettings : IDbSettings
    {
        public string DbName { get; set; }
        public string DbConnection { get; set; }
        public string UserCollectionName { get; set; }
        public string ChatGroupCollectionName { get; set; }
        public string ChatLogCollectionName { get; set; }
        public string ProfileCollectionName { get; set; }
        public string FileBucketName { get; set; }
    }

    public interface IDbSettings
    {
        string DbName { get; set; }
        string DbConnection { get; set; }
        string UserCollectionName { get; set; }
        string ChatGroupCollectionName { get; set; }
        string ChatLogCollectionName { get; set; }
        string ProfileCollectionName { get; set; }
        string FileBucketName { get; set; }
    }
}