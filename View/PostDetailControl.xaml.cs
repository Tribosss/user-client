using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

                string connStr = "server=localhost;database=mydb;user=root;password=1234;"; // 환경변수 대체
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
                    mainWindow.Content = new PostListControl(sharedViewModel); // ViewModel 공유
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("오류: " + ex.Message);
            }
        }


    }
}
