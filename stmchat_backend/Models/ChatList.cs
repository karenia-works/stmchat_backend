namespace stmchat_backend.Models
{
    public class ChatListItem
    {
        public ChatGroup group { get; set; }
        public int unreadCount { get; set; }
        public SendMessage message { get; set; }
    }
}
