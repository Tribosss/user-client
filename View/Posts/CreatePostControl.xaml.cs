using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using user_client.Model;
using user_client.ViewModel;

namespace user_client.View
{
    public partial class CreatePostControl : System.Windows.Controls.UserControl
    {
        public event Action<Post, PostViewModel>? PostCreated;
        private bool _isEditMode;
        private Post? _editingPost;
        private string? _originalTitle;
        private PostViewModel _vm;
        private string? _currentUserId;
        public CreatePostControl(PostViewModel vm, string currentUserId)
        {
            InitializeComponent();
            _vm = vm;
            _currentUserId = currentUserId;
        }
        public CreatePostControl(Post postToEdit, bool isEditMode, string currentUserId)
        {
            InitializeComponent();

            _isEditMode = isEditMode;
            _editingPost = postToEdit;

            if (_isEditMode && postToEdit != null)
            {
                // ✅ 새로운 복사본을 만들어서 수정 (참조 분리)
                _editingPost = new Post
                {
                    Id = postToEdit.Id,
                    Title = postToEdit.Title,
                    Body = postToEdit.Body,
                    Author = postToEdit.Author,
                    Date = postToEdit.Date,
                    Type = postToEdit.Type
                };
                _currentUserId = currentUserId;
                _originalTitle = postToEdit.Title;

                // 텍스트박스에 복사본 내용 표시
                TitleTextBox.Text = _editingPost.Title;
                BodyTextBox.Text = _editingPost.Body;

                // ✅ TypeComboBox도 설정해줍니다
                if (_editingPost.Type == "NOTICE")
                    TypeComboBox.SelectedIndex = 0;
                else
                    TypeComboBox.SelectedIndex = 1;
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

                PostCreated?.Invoke(_editingPost, _vm);
            }
            else
            {
                var newPost = new Post
                {
                    Title = TitleTextBox.Text,
                    Body = BodyTextBox.Text,
                    Date = DateTime.Now,
                    Author = _currentUserId,
                    Type = selectedType
                };
                InsertPostToDatabase(newPost);
                
                _vm.Posts.Insert(0, newPost);
                PostCreated?.Invoke(newPost, _vm);
            }
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
                    insertCmd.Parameters.AddWithValue("@author", post.Author);
                    insertCmd.Parameters.AddWithValue("@Type", post.Type ?? "notice");

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
                    string updateQuery = "UPDATE posts SET Title = @title, Body = @body, Type = @Type WHERE Id = @id";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);

                    updateCmd.Parameters.AddWithValue("@title", post.Title);
                    updateCmd.Parameters.AddWithValue("@body", post.Body);
                    updateCmd.Parameters.AddWithValue("@Type", post.Type ?? "notice");
                    updateCmd.Parameters.AddWithValue("@id", post.Id); // 만약 ID 필드가 있다면 ID로 수정 필요
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
