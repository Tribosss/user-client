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
        public Post Post
        {
            get => _post;
            set
            {
                if (_post == value) return;
                _post = value;
            }
        }

        public PostDetailControl(Post post, PostViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            _post = post;
            this.DataContext = this;
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var createPostControl = new CreatePostControl(_post,true);

            // 수정 완료 후 PostDetailControl로 다시 이동
            createPostControl.PostCreated += NavigatePostDetail.Invoke;

            NavigateCreatePost?.Invoke(_vm);
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

                NavigatePostList?.Invoke();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("오류: " + ex.Message);
            }
        }
    }
}
