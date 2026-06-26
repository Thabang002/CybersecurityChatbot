using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    public class TaskService
    {
        private ActivityLogService logService;

        public TaskService(ActivityLogService logService)
        {
            this.logService = logService;
        }

        public string ProcessTaskRequest(string input, out TaskItem task)
        {
            task = null;

            // Check if this is a task creation request
            if (IsTaskCreationRequest(input))
            {
                task = ParseTaskFromInput(input);
                if (task != null)
                {
                    var id = DatabaseService.AddTask(task);
                    task.Id = id;
                    logService.LogActivity("Task Added", $"Added task: {task.Title}");
                    return $"✅ Task added: '{task.Title}'\n" +
                           $"📝 Description: {task.Description}\n" +
                           (task.ReminderDate.HasValue ? $"⏰ Reminder set for: {task.ReminderDate.Value:MMMM dd, yyyy}" : "No reminder set") +
                           "\n\nWould you like to set a reminder for this task?";
                }
            }

            // Check for viewing tasks
            if (input.ToLower().Contains("show tasks") || input.ToLower().Contains("view tasks") ||
                input.ToLower().Contains("list tasks") || input.ToLower().Contains("my tasks"))
            {
                return GetTasksResponse();
            }

            // Check for completing tasks
            if (input.ToLower().Contains("complete task") || input.ToLower().Contains("mark done") ||
                input.ToLower().Contains("task done"))
            {
                return CompleteTask(input);
            }

            // Check for deleting tasks
            if (input.ToLower().Contains("delete task") || input.ToLower().Contains("remove task"))
            {
                return DeleteTask(input);
            }

            return null;
        }

        private bool IsTaskCreationRequest(string input)
        {
            string[] patterns = { "add task", "create task", "new task", "task to", "remind me to" };
            return patterns.Any(p => input.ToLower().Contains(p));
        }

        private TaskItem ParseTaskFromInput(string input)
        {
            var task = new TaskItem
            {
                Title = "",
                Description = "",
                CreatedDate = DateTime.Now,
                IsCompleted = false
            };

            // Extract title - take everything after "add task" or similar
            var titlePattern = @"(?i)(?:add task|create task|new task|task to|remind me to)\s*(.+?)(?:with|for|on|$|remind)";
            var titleMatch = Regex.Match(input, titlePattern, RegexOptions.IgnoreCase);

            if (titleMatch.Success)
            {
                task.Title = titleMatch.Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(task.Title))
                    return null;
            }
            else
            {
                // If no title found, use the whole input
                task.Title = input.Trim();
            }

            // Check for description
            var descMatch = Regex.Match(input, @"(?:about|for|with)\s*(.+?)(?:remind|on|$)", RegexOptions.IgnoreCase);
            if (descMatch.Success && descMatch.Groups[1].Value.Length > 10)
            {
                task.Description = descMatch.Groups[1].Value.Trim();
            }
            else
            {
                task.Description = GetDefaultDescription(task.Title);
            }

            // Check for reminder
            var reminderMatch = Regex.Match(input, @"(?:remind|in|on|after)\s*(\d+)\s*(day|days|week|weeks|month|months)", RegexOptions.IgnoreCase);
            if (reminderMatch.Success)
            {
                var number = int.Parse(reminderMatch.Groups[1].Value);
                var unit = reminderMatch.Groups[2].Value.ToLower();
                task.ReminderDate = unit.StartsWith("day") ? DateTime.Now.AddDays(number) :
                                    unit.StartsWith("week") ? DateTime.Now.AddDays(number * 7) :
                                    DateTime.Now.AddMonths(number);
            }

            return task;
        }

        private string GetDefaultDescription(string title)
        {
            return $"Cybersecurity task: {title}";
        }

        private string GetTasksResponse()
        {
            var tasks = DatabaseService.GetAllTasks();
            if (tasks.Count == 0)
                return "📋 You have no tasks yet. Add a cybersecurity task to get started!";

            var pending = tasks.Where(t => !t.IsCompleted).ToList();
            var completed = tasks.Where(t => t.IsCompleted).ToList();

            var response = "📋 **Your Cybersecurity Tasks**\n\n";

            if (pending.Count > 0)
            {
                response += "⏳ **Pending Tasks:**\n";
                foreach (var task in pending)
                {
                    response += $"  • {task.Title}\n";
                    response += $"    📝 {task.Description}\n";
                    if (task.ReminderDate.HasValue)
                        response += $"    ⏰ Reminder: {task.ReminderDate.Value:MMM dd, yyyy}\n";
                    response += $"    [Type 'Complete task {task.Id}' to mark as done]\n\n";
                }
            }

            if (completed.Count > 0)
            {
                response += "✅ **Completed Tasks:**\n";
                foreach (var task in completed.Take(5))
                {
                    response += $"  ✓ {task.Title}\n";
                }
                if (completed.Count > 5)
                    response += $"  ... and {completed.Count - 5} more completed tasks\n";
            }

            return response;
        }

        private string CompleteTask(string input)
        {
            var match = Regex.Match(input, @"(\d+)");
            if (!match.Success)
                return "Please specify the task ID to complete. Example: 'Complete task 1'";

            var id = int.Parse(match.Groups[1].Value);
            var tasks = DatabaseService.GetAllTasks();
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task == null)
                return $"No task found with ID {id}.";

            task.IsCompleted = true;
            DatabaseService.UpdateTask(task);
            logService.LogActivity("Task Completed", $"Completed task: {task.Title}");
            return $"✅ Task '{task.Title}' marked as completed! Great job! 🎉";
        }

        private string DeleteTask(string input)
        {
            var match = Regex.Match(input, @"(\d+)");
            if (!match.Success)
                return "Please specify the task ID to delete. Example: 'Delete task 1'";

            var id = int.Parse(match.Groups[1].Value);
            var tasks = DatabaseService.GetAllTasks();
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task == null)
                return $"No task found with ID {id}.";

            DatabaseService.DeleteTask(id);
            logService.LogActivity("Task Deleted", $"Deleted task: {task.Title}");
            return $"🗑️ Task '{task.Title}' has been deleted.";
        }
    }
}