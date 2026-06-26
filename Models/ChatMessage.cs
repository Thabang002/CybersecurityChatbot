using System;

namespace CybersecurityChatbot.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsUser { get; set; }
    }
}