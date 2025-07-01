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
using MySql.Data.MySqlClient;
using user_client.Model;
using user_client.ViewModel;
using DotNetEnv;

namespace user_client.View
{
    public partial class PostListControl : System.Windows.Controls.UserControl
    {
        private readonly PostViewModel _viewModel;
        public event Action<Post>? SelectPostEvent;
        public event Action? CreateEvent;
        public PostListControl()
        {
            InitializeComponent();

            _viewModel = this.DataContext as PostViewModel ?? new PostViewModel();
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
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
            {
                Console.WriteLine("Error: MainWindow not found.");
                return;
            }

            var postDetailControl = new PostDetailControl(selectedPost, () =>
            {
                mainWindow.NavigateTo(new PostListControl());
            });
            mainWindow.NavigateTo(postDetailControl);

            ////SelectPostEvent?.Invoke(selectedPost);
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
                {
                    Console.WriteLine("DB 환경 변수가 설정되지 않았습니다.");
                    return;
                }

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    string selectQuery = "SELECT id, title, body, created_at FROM posts";
                    MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);

                    using (MySqlDataReader rdr = selectCmd.ExecuteReader())
                    {
                        var posts = new ObservableCollection<Post>();
                        while (rdr.Read())
                        {
                            posts.Add(new Post
                            {
                                Id = rdr.GetInt32("id"),
                                Title = rdr.GetString("title"),
                                Body = rdr.GetString("body"),
                                CreatedAt = rdr.IsDBNull(rdr.GetOrdinal("created_at"))
                                ? DateTime.MinValue  // 기본값 설정 (예: DateTime.MinValue)
        :                       rdr.GetDateTime("created_at")
                            });

                        }

                        // ViewModel의 Posts 컬렉션 업데이트
                        _viewModel.Posts = posts;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"조회 오류: {ex.Message}");
            }
        }

        // 데이터베이스에 새 게시글 삽입
        public void InsertPost(string title, string body)
        {
            try
            {
                Env.Load();

                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                string? name = Environment.GetEnvironmentVariable("DB_NAME");
                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO posts (title, body, created_at) VALUES (@Title, @Body, @created_at)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@Title", title);
                    insertCmd.Parameters.AddWithValue("@Body", body);
                    insertCmd.Parameters.AddWithValue("@created_at", DateTime.Now);

                    if (insertCmd.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("성공적으로 게시글 삽입 완료.");
                        LoadPostsFromDatabase(); // 게시글 목록 갱신
                    }
                    else
                    {
                        Console.WriteLine("게시글 삽입 실패.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"삽입 오류: {ex.Message}");
            }
        }
    }
}
