namespace stmchat_backend.Models.Settings
{
    public class DbSettings : IDbSettings
    {
        public string DbName { get; set; }
        public string DbConnection { get; set; }
        public string UserCollectionName { get; set; }
        public string HouseCollectionName { get; set; }
        public string OrderCollectionName { get; set; }
        public string IssueCollectionName { get; set; }
        public string FinishedOrderCollectionName { get; set; }
        public string ImageBucketName { get; set; }
        public string ProfileCollectionName { get; set; }
    }

    public interface IDbSettings
    {
        string DbName { get; set; }
        string DbConnection { get; set; }
        string UserCollectionName { get; set; }
        string HouseCollectionName { get; set; }
        string OrderCollectionName { get; set; }
        string IssueCollectionName { get; set; }
        public string ImageBucketName { get; set; }
        string FinishedOrderCollectionName { get; set; }
        string ProfileCollectionName { get; set; }
    }
}