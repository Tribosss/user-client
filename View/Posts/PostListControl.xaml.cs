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
        private SideMenuControl _sideMenu;
        public event Action? NavigateToListRequested;
        public event Action? NavigateToChatRequested;


        public PostListControl(PostViewModel sharedViewModel)
        {
            InitializeComponent();

            _viewModel = sharedViewModel;
            this.DataContext = _viewModel;

            SideMenu.NavigateToListRequested += () => NavigateToListRequested?.Invoke();
            SideMenu.NavigateToChatRequested += () => NavigateToChatRequested?.Invoke();

            LoadPostsFromDatabase();
        }
        private void SideMenu_NavigateToListRequested()
        {
            // 게시판 버튼 눌렀을 때 동작할 내용 (보통 현재 PostListControl이니 별도 처리 안 해도 될 수도)
            // 하지만 MainWindow로 넘겨서 화면 전환을 원하면 아래처럼 호출

            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            mainWindow?.NavigateToPostList();
        }

        private void SideMenu_NavigateToChatRequested()
        {
            // 채팅 버튼 눌렀을 때 처리
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            mainWindow?.ContentArea.Children.Clear();
            mainWindow?.ContentArea.Children.Add(new ChatControl());
        }
        private void OnPostCreated(Post post)
        {
            _viewModel.AddPost(post); // ViewModel에 추가 + 마지막 페이지 이동
        }
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateToPostList(); // ← MainWindow에 이 메서드가 public이어야 함
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GotoChatEvnt?.Invoke();
        }
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CurrentPage > 1)
            {
                _viewModel.CurrentPage--;
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CurrentPage < _viewModel.TotalPages)
            {
                _viewModel.CurrentPage++;
            }
        }

    }
}