using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using System.Xml.Linq;
using user_client.Model;
using user_client.ViewModel;

namespace user_client.View
{
    /// <summary>
    /// Interaction logic for PostDetailControl.xaml
    /// </summary>
    public partial class PostDetailControl : System.Windows.Controls.UserControl
    {
        private Action _goBackCallback;
        private Post _currentPost;
        private PostViewModel sharedViewModel;

        public PostDetailControl(Post selectedPost, PostViewModel viewModel, Action goBackCallback)

        {
            InitializeComponent();
            _goBackCallback = goBackCallback;
            _currentPost = selectedPost;
            sharedViewModel = viewModel;
            this.DataContext = selectedPost;
        }
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _goBackCallback?.Invoke();
        }
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateToPostList(); // ← MainWindow에 이 메서드가 public이어야 함
            }
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            var createPostControl = new CreatePostControl(_currentPost, true);  // ← 이제 오류 안 남
            mainWindow.ContentArea.Children.Clear();
            mainWindow.ContentArea.Children.Add(createPostControl);
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var post = DataContext as Post;
                if (post == null)
                {
                    System.Windows.MessageBox.Show("게시글 정보 없음");
                    return;
                }
                Env.Load();
                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                string? name = Environment.GetEnvironmentVariable("DB_NAME");
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                string connStr = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";// 환경변수 대체
                using (var connection = new MySqlConnection(connStr))
                {
                    connection.Open();
                    string query = "DELETE FROM posts WHERE Id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", post.Id);
                    cmd.ExecuteNonQuery();
                }

                System.Windows.MessageBox.Show("삭제 완료");

                // 리스트 화면으로 이동
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    sharedViewModel.LoadPosts();
                    mainWindow.NavigateToPostList();
                    if (mainWindow.ContentArea.Children[0] is PostListControl postListControl)
                    {
                        // 기존 View 재사용 시 강제로 UI를 Refresh
                        postListControl.DataContext = null;
                        postListControl.DataContext = sharedViewModel;
                    }
                    else
                    {
                        mainWindow.ContentArea.Children.Clear();
                        mainWindow.ContentArea.Children.Add(new PostListControl(sharedViewModel));
                    }

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("오류: " + ex.Message);
            }
        }


    }
}
