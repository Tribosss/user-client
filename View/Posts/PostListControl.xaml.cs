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
        public event Action<Post>? SelectPostEvent;
        public event Action? GotoChatEvnt;
        public event Action? CreateEvent;
        public PostViewModel ViewModel => (PostViewModel)DataContext;
        public PostListControl()
        {
            InitializeComponent();
            Loaded += PostListControl_Loaded;
            _viewModel = this.DataContext as PostViewModel ?? new PostViewModel();
            this.DataContext = _viewModel;

        }
        private void PostListControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadPosts();  // ← 게시글 DB에서 로드
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            CreateEvent?.Invoke();

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
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is Post selectedPost)
            {
                // 선택된 게시글이 있으면 이벤트 호출
                SelectPostEvent?.Invoke(selectedPost);

                // ✅ 선택 상태 초기화 (같은 글을 다시 더블클릭할 수 있도록)
                dataGrid.SelectedItem = null;
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

                    string selectQuery = "SELECT Id, Title, Body, created_at, Author, Type FROM posts ORDER BY created_at DESC;";
                    MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                    MySqlDataReader rdr = selectCmd.ExecuteReader();

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
                    //connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading posts: " + ex.Message);
            }
        }
    }
}
