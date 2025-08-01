using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.ComponentModel;

namespace user_client.View
{
    /// <summary>
    /// Interaction logic for PostDetailControl.xaml
    /// </summary>
    public class Comment : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }

        private Visibility _deleteButtonVisibility = Visibility.Collapsed;
        public Visibility DeleteButtonVisibility
        {
            get => _deleteButtonVisibility;
            set
            {
                if (_deleteButtonVisibility != value)
                {
                    _deleteButtonVisibility = value;
                    OnPropertyChanged(nameof(DeleteButtonVisibility));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public partial class PostDetailControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public Action NavigatePostList;
        public Action<Post, PostViewModel> NavigatePostDetail;
        public Action<PostViewModel?> NavigateCreatePost;
        public PostViewModel _vm;
        private Post _post;
        private string _currentUserId;
        public event Action<Post> EditRequested;
        public ObservableCollection<Comment> Comments { get; set; } = new ObservableCollection<Comment>();

        public Post Post
        {
            get => _post;
            set
            {
                if (_post == value) return;
                _post = value;
                OnPropertyChanged(nameof(Post));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public PostDetailControl(Post post, PostViewModel vm, string currentUserId)
        {
            InitializeComponent();
            _vm = vm;
            _post = post;
            _currentUserId = currentUserId;
            this.DataContext = this;

            SetButtonVisibility();
            LoadComments();
        }
        private void UpdateCommentCount()
        {
            CommentCountText.Text = $"댓글 ({Comments.Count})";
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CommentInput.Text))
            {
                string content = CommentInput.Text;
                string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

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

                        string insertQuery = "INSERT INTO Comment (PostId, Author, Content, Date) VALUES (@postId, @Author, @Content, @Date)";
                        using (var cmd = new MySqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@postId", _post.Id);
                            cmd.Parameters.AddWithValue("@Author", _currentUserId);
                            cmd.Parameters.AddWithValue("@Content", content);
                            cmd.Parameters.AddWithValue("@Date", DateTime.Now);

                            cmd.ExecuteNonQuery();

                            // 방금 추가된 comment의 ID 가져오기
                            long lastId = cmd.LastInsertedId;

                            var newComment = new Comment
                            {
                                Id = (int)lastId,
                                Author = _currentUserId,
                                Content = content,
                                Date = dateStr
                            };
                            SetCommentDeleteButtonVisibility(newComment);
                            Comments.Add(newComment);
                        }
                    }

                    CommentInput.Text = string.Empty;
                    UpdateCommentCount();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("댓글 저장 오류: " + ex.Message);
                }
            }
        }
        private void DeleteComment(int commentId)
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

                string deleteQuery = "DELETE FROM Comment WHERE Id = @id";
                using (var cmd = new MySqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@id", commentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button == null) return;

            var comment = button.DataContext as Comment;
            if (comment == null) return;

            var result = System.Windows.MessageBox.Show("댓글을 삭제하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                DeleteComment(comment.Id); // Id 기준 삭제

                Comments.Remove(comment);
                UpdateCommentCount();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("댓글 삭제 오류: " + ex.Message);
            }
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
        private void SetCommentDeleteButtonVisibility(Comment comment)
        {
            // 현재 로그인한 사용자 _currentUserId
            // 게시글 작성자 _post.Author

            if (_currentUserId == _post.Author || _currentUserId == comment.Author)
            {
                comment.DeleteButtonVisibility = Visibility.Visible;
            }
            else
            {
                comment.DeleteButtonVisibility = Visibility.Collapsed;
            }
        }
        private void LoadComments()
        {
            try
            {
                Comments.Clear();
                Env.Load();
                string connStr = $"Server={Environment.GetEnvironmentVariable("DB_HOST")};" +
                                 $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                                 $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                                 $"Uid={Environment.GetEnvironmentVariable("DB_UID")};" +
                                 $"Pwd={Environment.GetEnvironmentVariable("DB_PWD")}";

                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT Id, Author, Content, Date FROM Comment WHERE PostId = @postId ORDER BY Date ASC\r\n";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@postId", _post.Id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var comment = new Comment
                                {
                                    Id = reader.GetInt32("Id"),
                                    Author = reader.GetString("Author"),
                                    Content = reader.GetString("Content"),
                                    Date = reader.GetDateTime("Date").ToString("yyyy-MM-dd HH:mm")
                                };
                                SetCommentDeleteButtonVisibility(comment);
                                Comments.Add(comment);
                            }
                        }
                    }
                }
                UpdateCommentCount();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("댓글 불러오기 오류: " + ex.Message);
            }
        }

    }
}
