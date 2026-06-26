using System;

namespace CybersecurityChatbot.Models
{
    public class ActivityLogEntry
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string Category { get; set; }
    }
}