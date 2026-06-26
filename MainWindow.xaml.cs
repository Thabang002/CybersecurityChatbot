using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbot.Models;
using CybersecurityChatbot.Services;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private ChatbotService chatbot;
        private TaskService taskService;
        private QuizService quizService;
        private ActivityLogService logService;
        private List<TaskItem> tasks;

        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            LoadTasks();
            LoadQuizHistory();
            ShowWelcomeMessage();
        }

        private void InitializeServices()
        {
            logService = new ActivityLogService();
            chatbot = new ChatbotService(logService);
            taskService = new TaskService(logService);
            quizService = new QuizService(logService);

            // Update status bar
            UpdateStatusBar();
        }

        private void ShowWelcomeMessage()
        {
            AddBotMessage("👋 Welcome to the Cybersecurity Awareness Chatbot!\n\n" +
                         "I'm here to help you learn about online safety. Here's what I can do:\n\n" +
                         "🔐 **Cybersecurity Tips** - Ask me about passwords, scams, privacy, phishing, 2FA, and more\n" +
                         "📋 **Task Manager** - Add and manage your cybersecurity tasks\n" +
                         "📝 **Quiz** - Test your cybersecurity knowledge\n" +
                         "💡 **Personalized Help** - Tell me your name or what interests you!\n\n" +
                         "Type a message to get started! 🇿🇦");
        }

        private void AddBotMessage(string message)
        {
            AddMessage(message, false);
        }

        private void AddUserMessage(string message)
        {
            AddMessage(message, true);
        }

        private void AddMessage(string message, bool isUser)
        {
            var border = new Border
            {
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(8),
                MaxWidth = 500,
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Background = isUser ? (Brush)FindResource("ChatUserColor") : (Brush)FindResource("ChatBotColor")
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = isUser ? Brushes.Black : Brushes.White,
                FontSize = 13,
                LineHeight = 1.4
            };

            border.Child = textBlock;
            ChatMessagesPanel.Children.Add(border);

            // Scroll to bottom
            ChatScrollViewer.ScrollToBottom();
        }

        private void ProcessUserInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            AddUserMessage(input);

            // Check if input is for quiz
            if (input.ToLower().Contains("start quiz") ||
                (quizService.IsActive && (input.Length == 1 || input.Length == 2)))
            {
                if (quizService.IsActive)
                {
                    var response = quizService.ProcessAnswer(input);
                    AddBotMessage(response);
                    LoadQuizHistory();
                    UpdateStatusBar();
                    return;
                }
                else
                {
                    var response = quizService.StartQuiz();
                    AddBotMessage(response);
                    LoadQuizHistory();
                    UpdateStatusBar();
                    return;
                }
            }

            // Check if input is for task management
            var taskResponse = taskService.ProcessTaskRequest(input, out TaskItem task);
            if (taskResponse != null)
            {
                AddBotMessage(taskResponse);
                LoadTasks();
                UpdateStatusBar();
                return;
            }

            // Process with chatbot
            var response = chatbot.ProcessMessage(input);
            AddBotMessage(response);
            UpdateStatusBar();

            // Check if quiz was mentioned
            if (input.ToLower().Contains("quiz") && !input.ToLower().Contains("start"))
            {
                AddBotMessage("📝 To start the quiz, type 'Start quiz' and I'll test your cybersecurity knowledge!");
            }
        }

        private void LoadTasks()
        {
            tasks = DatabaseService.GetAllTasks();
            TasksListBox.ItemsSource = tasks;
            UpdateStatusBar();
        }

        private void LoadQuizHistory()
        {
            var history = DatabaseService.GetQuizHistory();
            QuizHistoryListBox.ItemsSource = history.Select(h =>
                $"Score: {h.Score}/{h.Total} - {h.Date:MMM dd, yyyy HH:mm}");
        }

        private void UpdateStatusBar()
        {
            var userProfile = chatbot.GetUserProfile();
            UserInfoText.Text = string.IsNullOrEmpty(userProfile.Name) ? "👤 Guest" : $"👤 {userProfile.Name}";

            var topic = chatbot.GetCurrentTopic();
            TopicInfoText.Text = string.IsNullOrEmpty(topic) ? "💡 Ask about cybersecurity!" : $"💡 Topic: {topic}";

            QuizStatusBarText.Text = quizService.IsActive ?
                $"📝 Quiz in progress ({quizService.CurrentQuestionNumber}/{quizService.TotalQuestions})" :
                "📝 Quiz inactive";

            var pendingTasks = tasks?.Count(t => !t.IsCompleted) ?? 0;
            StatusText.Text = pendingTasks > 0 ?
                $"🔄 {pendingTasks} pending tasks | Ready 🇿🇦" :
                "Ready 🇿🇦";
        }

        // Event Handlers
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var input = MessageTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                ProcessUserInput(input);
                MessageTextBox.Clear();
            }
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                SendButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput("Start quiz");
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatMessagesPanel.Children.Clear();
            ShowWelcomeMessage();
        }

        private void ShowActivityLog_Click(object sender, RoutedEventArgs e)
        {
            var logs = logService.GetFormattedLogs(10);
            AddBotMessage(logs);
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var title = TaskTitleTextBox.Text.Trim();
            var description = TaskDescriptionTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                AddBotMessage("⚠️ Please enter a task title.");
                return;
            }

            var task = new TaskItem
            {
                Title = title,
                Description = string.IsNullOrEmpty(description) ? GetDefaultDescription(title) : description,
                CreatedDate = DateTime.Now,
                IsCompleted = false
            };

            if (HasReminderCheckBox.IsChecked == true && ReminderDatePicker.SelectedDate.HasValue)
            {
                task.ReminderDate = ReminderDatePicker.SelectedDate.Value;
            }

            var id = DatabaseService.AddTask(task);
            task.Id = id;
            logService.LogActivity("Task Added", $"Added task: {task.Title}");

            AddBotMessage($"✅ Task added: '{task.Title}'\n" +
                         $"📝 {task.Description}\n" +
                         (task.ReminderDate.HasValue ? $"⏰ Reminder: {task.ReminderDate.Value:MMMM dd, yyyy}" : "No reminder set"));

            LoadTasks();
            UpdateStatusBar();

            // Clear fields
            TaskTitleTextBox.Clear();
            TaskDescriptionTextBox.Clear();
            HasReminderCheckBox.IsChecked = false;
            ReminderDatePicker.SelectedDate = DateTime.Now;
        }

        private string GetDefaultDescription(string title)
        {
            return $"Cybersecurity task: {title}";
        }

        private void TasksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TasksListBox.SelectedItem is TaskItem selectedTask)
            {
                var result = MessageBox.Show($"Task: {selectedTask.Title}\n" +
                                            $"Description: {selectedTask.Description}\n" +
                                            $"Created: {selectedTask.CreatedDate:MMM dd, yyyy}\n" +
                                            $"Status: {(selectedTask.IsCompleted ? "✅ Completed" : "⏳ Pending")}\n\n" +
                                            "Mark as completed?",
                                            "Task Details",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    selectedTask.IsCompleted = true;
                    DatabaseService.UpdateTask(selectedTask);
                    logService.LogActivity("Task Completed", $"Completed task: {selectedTask.Title}");
                    LoadTasks();
                    UpdateStatusBar();
                    AddBotMessage($"✅ Task '{selectedTask.Title}' marked as completed! 🎉");
                }

                TasksListBox.SelectedItem = null;
            }
        }
    }
}