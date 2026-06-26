using CybersecurityChatbot.Models;
using System.Data.SQLite;
using System.IO;

namespace CybersecurityChatbot.Services
{
    public static class DatabaseService
    {
        private static string connectionString;

        public static void InitializeDatabase()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dbPath))
                Directory.CreateDirectory(dbPath);

            connectionString = $"Data Source={Path.Combine(dbPath, "chatbot_data.db")};Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Create tasks table
                string createTasksTable = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        ReminderDate TEXT,
                        IsCompleted INTEGER DEFAULT 0,
                        CreatedDate TEXT NOT NULL
                    )";

                using (var cmd = new SQLiteCommand(createTasksTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Create activity log table
                string createActivityLogTable = @"
                    CREATE TABLE IF NOT EXISTS ActivityLog (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Action TEXT NOT NULL,
                        Description TEXT,
                        Timestamp TEXT NOT NULL,
                        Category TEXT
                    )";

                using (var cmd = new SQLiteCommand(createActivityLogTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Create quiz results table
                string createQuizResultsTable = @"
                    CREATE TABLE IF NOT EXISTS QuizResults (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Score INTEGER,
                        TotalQuestions INTEGER,
                        Date TEXT NOT NULL
                    )";

                using (var cmd = new SQLiteCommand(createQuizResultsTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Task Operations
        public static int AddTask(TaskItem task)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO Tasks (Title, Description, ReminderDate, IsCompleted, CreatedDate)
                    VALUES (@Title, @Description, @ReminderDate, @IsCompleted, @CreatedDate);
                    SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Title", task.Title);
                    cmd.Parameters.AddWithValue("@Description", task.Description ?? "");
                    cmd.Parameters.AddWithValue("@ReminderDate", task.ReminderDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                    cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted ? 1 : 0);
                    cmd.Parameters.AddWithValue("@CreatedDate", task.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Tasks ORDER BY IsCompleted, CreatedDate DESC";

                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new TaskItem
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            ReminderDate = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
                            IsCompleted = reader.GetInt32(4) == 1,
                            CreatedDate = DateTime.Parse(reader.GetString(5))
                        });
                    }
                }
            }
            return tasks;
        }

        public static void UpdateTask(TaskItem task)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    UPDATE Tasks 
                    SET Title = @Title, Description = @Description, 
                        ReminderDate = @ReminderDate, IsCompleted = @IsCompleted
                    WHERE Id = @Id";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", task.Id);
                    cmd.Parameters.AddWithValue("@Title", task.Title);
                    cmd.Parameters.AddWithValue("@Description", task.Description ?? "");
                    cmd.Parameters.AddWithValue("@ReminderDate", task.ReminderDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                    cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted ? 1 : 0);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteTask(int taskId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Tasks WHERE Id = @Id";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", taskId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Activity Log Operations
        public static void AddActivityLog(ActivityLogEntry entry)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO ActivityLog (Action, Description, Timestamp, Category)
                    VALUES (@Action, @Description, @Timestamp, @Category)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Action", entry.Action);
                    cmd.Parameters.AddWithValue("@Description", entry.Description ?? "");
                    cmd.Parameters.AddWithValue("@Timestamp", entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@Category", entry.Category ?? "General");

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<ActivityLogEntry> GetRecentActivityLogs(int count = 10)
        {
            var logs = new List<ActivityLogEntry>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM ActivityLog ORDER BY Timestamp DESC LIMIT @Count";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Count", count);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new ActivityLogEntry
                            {
                                Id = reader.GetInt32(0),
                                Action = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Timestamp = DateTime.Parse(reader.GetString(3)),
                                Category = reader.IsDBNull(4) ? "General" : reader.GetString(4)
                            });
                        }
                    }
                }
            }
            return logs;
        }

        // Quiz Operations
        public static void SaveQuizResult(int score, int totalQuestions)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO QuizResults (Score, TotalQuestions, Date)
                    VALUES (@Score, @TotalQuestions, @Date)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Score", score);
                    cmd.Parameters.AddWithValue("@TotalQuestions", totalQuestions);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<(int Score, int Total, DateTime Date)> GetQuizHistory()
        {
            var history = new List<(int, int, DateTime)>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Score, TotalQuestions, Date FROM QuizResults ORDER BY Date DESC LIMIT 10";

                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        history.Add((
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            DateTime.Parse(reader.GetString(2))
                        ));
                    }
                }
            }
            return history;
        }
    }
}