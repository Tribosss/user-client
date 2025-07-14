using DotNetEnv;
using System;
using System.Windows;
using System.Windows.Controls;
using user_client.Model;
using user_client.ViewModel;
using MySql.Data.MySqlClient;
using user_client.Components; // ✅ SideBarControl 사용을 위한 네임스페이스

namespace user_client.View
{
    public partial class CreatePostControl : System.Windows.Controls.UserControl
    {
        public event Action<Post>? PostCreated;

        private readonly PostViewModel _viewModel; // 전달받은 ViewModel
        private readonly UserData _userData;       // 로그인 사용자 정보
        private bool _isEditMode;
        private Post? _editingPost;
        private string? _originalTitle;

        private SideBarControl _sideBar; // ✅ 사이드바 참조

        // ✅ 새 게시글 작성용 생성자
        public CreatePostControl(PostViewModel viewModel, UserData userData)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _userData = userData;

            _sideBar = new SideBarControl(_userData);
            SideBarPlaceholder.Content = _sideBar;

            _sideBar.BoardNavigateEvt += OnNavigateToListRequested;
            _sideBar.ShowChatWindowEvt += _ => OnNavigateToChatRequested();
        }

        // ✅ 게시글 수정용 생성자
        public CreatePostControl(PostViewModel viewModel, Post postToEdit, bool isEditMode, UserData userData)
            : this(viewModel, userData)
        {
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
            mainWindow?.NavigateToChat();
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
                    Author = _userData.Id, // ✅ 로그인한 사용자 ID
                    Type = selectedType
                };

                InsertPostToDatabase(newPost);

                _viewModel.AllPosts.Insert(0, newPost);
                _viewModel.CurrentPage = _viewModel.TotalPages;

                PostCreated?.Invoke(newPost);
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

                using (var connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO posts (Title, Body, created_at, Author, Type) VALUES (@title, @body, @date, @author, @type)";
                    var cmd = new MySqlCommand(insertQuery, connection);
                    cmd.Parameters.AddWithValue("@title", post.Title);
                    cmd.Parameters.AddWithValue("@body", post.Body);
                    cmd.Parameters.AddWithValue("@date", post.Date);
                    cmd.Parameters.AddWithValue("@author", post.Author);
                    cmd.Parameters.AddWithValue("@type", post.Type);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Insert Error: " + ex.Message);
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

                if (host == null || port == null || uid == null || pwd == null || name == null)
                    return;

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (var connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();
                    string updateQuery = "UPDATE posts SET Title = @title, Body = @body, Type = @type WHERE Title = @originalTitle";
                    var cmd = new MySqlCommand(updateQuery, connection);
                    cmd.Parameters.AddWithValue("@title", post.Title);
                    cmd.Parameters.AddWithValue("@body", post.Body);
                    cmd.Parameters.AddWithValue("@type", post.Type);
                    cmd.Parameters.AddWithValue("@originalTitle", _originalTitle);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update Error: " + ex.Message);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            mainWindow?.NavigateToPostList();
        }
    }
}
