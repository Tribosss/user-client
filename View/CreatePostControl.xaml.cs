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

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = TitleTextBox.Text;
                string body = BodyTextBox.Text;

                if (InsertPost(title, body))
                {
                    var newPost = new Post
                    {
                        Title = title,
                        Body = body,
                    };

                    PostCreated?.Invoke(newPost);

                    var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                    if (mainWindow == null)
                    {
                        Console.WriteLine("Error: MainWindow not found.");
                        return;
                    }

                    var postDetailControl = new PostDetailControl(newPost, () =>
                    {
                        Console.WriteLine("Returning to PostListControl...");
                        mainWindow.NavigateTo(new PostListControl());
                    });

                    mainWindow.NavigateTo(postDetailControl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }     
        // 데이터 삽입 메서드
        private bool InsertPost(string title, string body)
        {
            try
            {
                // 환경 변수에서 DB 설정 읽기
                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                string? name = Environment.GetEnvironmentVariable("DB_NAME");

                if (host == null || port == null || uid == null || pwd == null || name == null)
                    throw new Exception("데이터베이스 설정이 누락되었습니다.");

                string connectionString = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO posts (id, title, body, created_at) VALUES (@Id, @Title, @Body, NOW())";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", 5);
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Body", body);

                    return command.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Insert Error: {ex.Message}");
                return false;
            }
        }

        // 데이터 조회 메서드
        private void SelectPosts()
        {
            try
            {
                // 환경 변수에서 DB 설정 읽기
                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                string? name = Environment.GetEnvironmentVariable("DB_NAME");
                string connectionString = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM posts";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Title: {reader["title"]}, Body: {reader["body"]}, Date: {reader["date"]}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Select Error: {ex.Message}");
            }
        }
    }
}
