using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using user_client.Model;
using user_client.ViewModel;
using MySql.Data.MySqlClient;

namespace user_client.View
{
    public partial class PostListControl : System.Windows.Controls.UserControl
    {
        private readonly PostViewModel _viewModel;
        public event Action<user_client.Model.Post>? SelectPostEvent;
        public event Action? GotoChatEvnt;
        public event Action? CreateEvent;
        public PostListControl(PostViewModel sharedViewModel)
        {
            InitializeComponent();

            _viewModel = sharedViewModel;
            this.DataContext = _viewModel;

            LoadPostsFromDatabase();
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            CreateEvent?.Invoke();

        }
        private void PostList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            Post? selectedPost = _viewModel.SelectedPost;
            if (selectedPost == null) return;
            SelectPostEvent?.Invoke(selectedPost);
        }
        public void LoadPostsFromDatabase()
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

                    string selectQuery = "SELECT Title, Body, Date, Author, Status FROM Post ORDER BY Date DESC;";
                    MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                    MySqlDataReader rdr = selectCmd.ExecuteReader();

                    _viewModel.Posts.Clear(); // 기존 데이터 초기화

                    while (rdr.Read())
                    {
                        Post post = new Post
                        {
                            Title = rdr.GetString("title"),
                            Body = rdr.GetString("Body"),
                            Date = rdr.GetDateTime("Date"),
                            Author = rdr.IsDBNull(rdr.GetOrdinal("Author")) ? "익명" : rdr.GetString("Author"),
                            Status = rdr.IsDBNull(rdr.GetOrdinal("Status")) ? "일반" : rdr.GetString("Status")
                        };

                        _viewModel.Posts.Add(post);
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading posts: " + ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GotoChatEvnt?.Invoke();
        }
    }
}
