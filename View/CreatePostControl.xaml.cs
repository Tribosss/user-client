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
        private bool _isEditMode;
        private Post? _editingPost;
        private string? _originalTitle;

        public CreatePostControl()
        {
            InitializeComponent();
            SideMenu.NavigateToListRequested += OnNavigateToListRequested;
            SideMenu.NavigateToChatRequested += OnNavigateToChatRequested;
        }
        public CreatePostControl(Post postToEdit, bool isEditMode)
        {
            InitializeComponent();

            _isEditMode = isEditMode;
            _editingPost = postToEdit;

            if (_isEditMode && _editingPost != null)
            {
                TitleTextBox.Text = _editingPost.Title;
                BodyTextBox.Text = _editingPost.Body;
                _originalTitle = _editingPost.Title;
            }
        }
        private void OnNavigateToListRequested()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            mainWindow?.NavigateToPostList();
        }

        private void OnNavigateToChatRequested()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            mainWindow?.ContentArea.Children.Clear();
            mainWindow?.ContentArea.Children.Add(new ChatControl());
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
                    string insertQuery = "INSERT INTO posts (Title, Body, created_at, Author, Type) VALUES (@title, @body, @date, @author, @Type)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@title", post.Title);
                    insertCmd.Parameters.AddWithValue("@body", post.Body);
                    insertCmd.Parameters.AddWithValue("@date", post.Date);
                    insertCmd.Parameters.AddWithValue("@author", post.Author ?? "익명");
                    insertCmd.Parameters.AddWithValue("@Type", post.Type ?? "NORMAL");

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
            string selectedType = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "NORMAL";

            if (_isEditMode && _editingPost != null)
            {
                _editingPost.Title = TitleTextBox.Text;
                _editingPost.Body = BodyTextBox.Text;
                _editingPost.Type = selectedType;
                UpdatePostInDatabase(_editingPost);

                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                mainWindow?.NavigateToPostList();
            }
            else
            {
                var newPost = new Post
                {
                    Title = TitleTextBox.Text,
                    Body = BodyTextBox.Text,
                    Date = DateTime.Now,
                    Author = "익명",
                    Type = selectedType
                };
                InsertPostToDatabase(newPost);
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    var viewModel = mainWindow.SharedViewModel;
                    viewModel.AllPosts.Insert(0, newPost); 
                    viewModel.CurrentPage = viewModel.TotalPages;
                }
                PostCreated?.Invoke(newPost);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateToPostList();
            }
        }
        private void UpdatePostInDatabase(Post post)
        {
            try
            {
                Env.Load();

                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                string? name = Environment.GetEnvironmentVariable("DB_NAME");

                if (host == null || port == null || uid == null || pwd == null || name == null) return;

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();
                    string updateQuery = "UPDATE posts SET Title = @title, Body = @body, Type = @Type WHERE Title = @originalTitle";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);

                    updateCmd.Parameters.AddWithValue("@title", post.Title);
                    updateCmd.Parameters.AddWithValue("@body", post.Body);
                    updateCmd.Parameters.AddWithValue("@Type", post.Type ?? "NORMAL"); 
                    updateCmd.Parameters.AddWithValue("@originalTitle", _originalTitle); // 만약 ID 필드가 있다면 ID로 수정 필요
                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    if (rowsAffected == 1)
                        Console.WriteLine("Success Update");
                    else
                        Console.WriteLine("Failed Update");

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update error: " + ex.Message);
            }
        }

    }
}
