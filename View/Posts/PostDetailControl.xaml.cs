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
        public Action NavigatePostList;
        public Action<Post, PostViewModel> NavigatePostDetail;
        public Action<PostViewModel?> NavigateCreatePost;
        public PostViewModel _vm;
        private Post _post;
        private string _currentUserId;
        public event Action<Post> EditRequested;
        public event Action<int> DeleteRequested;

        public Post Post
        {
            get => _post;
            set
            {
                if (_post == value) return;
                _post = value;
            }
        }

        public PostDetailControl(Post post, PostViewModel vm, string currentUserId)
        {
            InitializeComponent();
            _vm = vm;
            _post = post;
            _currentUserId = currentUserId;
            this.DataContext = this;

            SetButtonVisibility();
        }
        private void SetButtonVisibility()
        {
            if (_post.Author != _currentUserId)
            {
                EditButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                EditButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditRequested?.Invoke(_post);
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DeletePost(_post.Id);
                System.Windows.MessageBox.Show("삭제 완료");
                NavigatePostList?.Invoke();
            }
        }
        private void DeletePost(int postId)
        {
            try
            {
                Env.Load();
                string connStr = $"Server={Environment.GetEnvironmentVariable("DB_HOST")};" +
                                 $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                                 $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                                 $"Uid={Environment.GetEnvironmentVariable("DB_UID")};" +
                                 $"Pwd={Environment.GetEnvironmentVariable("DB_PWD")}";

                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "DELETE FROM posts WHERE Id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", postId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("삭제 오류: " + ex.Message);
            }
        }
    }
}
