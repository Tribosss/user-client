using DotNetEnv;
using MySql.Data.MySqlClient;
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

namespace user_client.View
{
    public partial class PostListControl : System.Windows.Controls.UserControl
    {
        private readonly PostViewModel _viewModel;
        public event Action<Post, PostViewModel>? SelectPostEvent;
        public event Action? GotoChatEvnt;
        public event Action<PostViewModel>? CreateEvent;
        public PostListControl()
        {
            InitializeComponent();

            _viewModel = this.DataContext as PostViewModel ?? new PostViewModel();
            this.DataContext = _viewModel;

            LoadPostsFromDatabase();
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            CreateEvent?.Invoke(_viewModel);
        }
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as PostViewModel;
            if (vm != null && vm.CurrentPage > 1)
            {
                vm.CurrentPage--;
                vm.UpdatePostsForCurrentPage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as PostViewModel;
            if (vm != null && vm.CurrentPage < vm.TotalPages)
            {
                vm.CurrentPage++;
                vm.UpdatePostsForCurrentPage();
            }
        }

        private void PostList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as System.Windows.Controls.ListView; // ✅ DataGrid → ListView로 수정
            if (listView?.SelectedItem is Post selectedPost)
            {
                SelectPostEvent?.Invoke(selectedPost, _viewModel);

                // ✅ 선택 상태 초기화
                listView.SelectedItem = null;
                _viewModel.SelectedPost = null;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GotoChatEvnt?.Invoke();
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

                    string selectQuery = "SELECT Id, Title, Body, created_at, Author, Type FROM posts ORDER BY CASE WHEN type = 'notice' THEN 0 ELSE 1 END, created_at DESC;";
                    MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                    MySqlDataReader rdr = selectCmd.ExecuteReader();

                    //if (_viewModel)
                    _viewModel.AllPosts.Clear();

                    while (rdr.Read())
                    {
                        Post post = new Post
                        {
                            Id = rdr.GetInt32("Id"),
                            Title = rdr.GetString("Title"),
                            Body = rdr.GetString("Body"),
                            Date = rdr.GetDateTime("created_at"),
                            Author = rdr.IsDBNull(rdr.GetOrdinal("Author")) ? "익명" : rdr.GetString("Author"),
                            Type = rdr.IsDBNull(rdr.GetOrdinal("Type")) ? "NORMAL" : rdr.GetString("Type")
                        };

                        _viewModel.AllPosts.Add(post);
                    }
                    _viewModel.CurrentPage = 1;
                    _viewModel.UpdatePostsForCurrentPage();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading posts: " + ex.Message);
            }
        }
    }
}
