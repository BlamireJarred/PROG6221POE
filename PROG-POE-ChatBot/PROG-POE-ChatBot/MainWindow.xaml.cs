using PROG_POE_ChatBot;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CyberAssistant
{
    public partial class MainWindow : Window
    {
        private List<TaskItem> tasks = new List<TaskItem>();
        private ChatBot bot;
        private Random rand = new Random();

        public MainWindow()
        {
            InitializeComponent();
            RefreshTaskList();
            bot = new ChatBot("bot");
        }

        private void HandleChatInput(string input)
        {
            AppendChat($"You: {input}");
            var result = bot.ProcessInput(input, tasks);
            AppendChat($"Bot: {result.Message}");

            if (result.CreatedTask != null && !tasks.Contains(result.CreatedTask))
            {
                tasks.Add(result.CreatedTask);
                RefreshTaskList();
            }
        }

        private void AppendChat(string message)
        {
            ChatLog.Text += message + Environment.NewLine;
        }

        private void SendChat_Click(object sender, RoutedEventArgs e)
        {
            string input = ChatInput.Text;
            ChatInput.Clear();
            RefreshTaskList();
            HandleChatInput(input);
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem is TaskItem task)
            {
                task.IsCompleted = true;
                RefreshTaskList();
                ActivityLogger.Log($"Task marked as completed: \"{task.Title}\"");
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem is TaskItem task)
            {
                tasks.Remove(task);
                RefreshTaskList();
                ActivityLogger.Log($"Task deleted: \"{task.Title}\"");
            }
        }

        private void RefreshTaskList()
        {
            TaskList.ItemsSource = null;
            TaskList.ItemsSource = tasks;
        }

        private void TaskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskList.SelectedItem is TaskItem task)
            {
                string status = task.ReminderDate.HasValue
                    ? (task.IsCompleted ? "Completed" : "Pending")
                    : "No reminder set";

                MessageBox.Show(
                    $"Title: {task.Title}\nDescription: {task.Description}\nReminder: {task.ReminderDate?.ToString("f") ?? "None"}\nStatus: {status}",
                    "Task Details",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}
