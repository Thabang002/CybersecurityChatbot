using System;
using System.Collections.Generic;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    public class ActivityLogService
    {
        public void LogActivity(string action, string description, string category = "General")
        {
            var entry = new ActivityLogEntry
            {
                Action = action,
                Description = description,
                Timestamp = DateTime.Now,
                Category = category
            };

            DatabaseService.AddActivityLog(entry);
        }

        public List<ActivityLogEntry> GetRecentLogs(int count = 10)
        {
            return DatabaseService.GetRecentActivityLogs(count);
        }

        public string GetFormattedLogs(int count = 10)
        {
            var logs = DatabaseService.GetRecentActivityLogs(count);
            if (logs.Count == 0)
                return "No activities logged yet.";

            var response = "📋 **Activity Log**\n\n";
            for (int i = 0; i < logs.Count; i++)
            {
                response += $"{i + 1}. **{logs[i].Action}**\n";
                response += $"   {logs[i].Description}\n";
                response += $"   {logs[i].Timestamp:MMM dd, HH:mm}\n\n";
            }

            return response;
        }
    }
}