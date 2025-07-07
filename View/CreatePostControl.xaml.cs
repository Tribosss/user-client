using DotNetEnv;
using System;
using System.Windows;
using System.Windows.Controls;
using user_client.Model;
using MySql.Data.MySqlClient;

namespace user_client.View
{
    public partial class CreatePostControl : System.Windows.Controls.UserControl
    {
        public event Action<Post>? PostCreated;

        public CreatePostControl()
        {
            InitializeComponent();
        }
        public void InsertPostToDatabase(Post post)
        {
            try
            {
                Env.Load();

                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                string? name = Environment.GetEnvironmentVariable("DB_NAME");

                if (host == null || port == null || uid == null || pwd == null || name == null)
                    return;

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO Post (Title, Body, Date, Author, Status) VALUES (@title, @body, @date, @author, @status)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@title", post.Title);
                    insertCmd.Parameters.AddWithValue("@body", post.Body);
                    insertCmd.Parameters.AddWithValue("@date", post.Date);
                    insertCmd.Parameters.AddWithValue("@author", post.Author ?? "익명");
                    insertCmd.Parameters.AddWithValue("@status", post.Status ?? "일반");

                    if (insertCmd.ExecuteNonQuery() == 1)
                        Console.WriteLine("Success Insert");
                    else
                        Console.WriteLine("Failed Insert");

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Insert error: " + ex.Message);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var newPost = new Post
            {
                Title = TitleTextBox.Text,
                Body = BodyTextBox.Text,
                Date = DateTime.Now,
                Author="익명",
                Status="일반"
            };
            InsertPostToDatabase(newPost);
            PostCreated?.Invoke(newPost);
        }
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateToPostList();
            }
        }

    }
}
