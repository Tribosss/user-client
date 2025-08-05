using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using user_client.Model;
using MySql.Data.MySqlClient;
using DotNetEnv;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace user_client.ViewModel
{
    public class PostViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Post> AllPosts { get; set; } = new ObservableCollection<Post>();
        public ObservableCollection<Post> Posts { get; set; } = new ObservableCollection<Post>();
        public ObservableCollection<int> PageNumbers { get; set; } = new ObservableCollection<int>();
        private int _totalPostCount;
        private int _currentPage = 1;
        private const int PageSize = 15;
        public int TotalPostCount
        {
            get => _totalPostCount;
            set
            {
                _totalPostCount = value;
                OnPropertyChanged(nameof(TotalPostCount));
            }
        }
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                    UpdatePostsForCurrentPage();
                }
            }
        }
        public int TotalPages => (int)Math.Ceiling((double)AllPosts.Count / PageSize);
        public void UpdatePostsForCurrentPage()
        {
            Posts.Clear();
            var pageItems = AllPosts
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            foreach (var post in pageItems)
            {
                Posts.Add(post);
            }
            UpdatePageNumbers();
            OnPropertyChanged(nameof(Posts));
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(TotalPages));
        }
        public void AddPost(Post post)
        {
            AllPosts.Insert(0, post);
            OnPropertyChanged(nameof(AllPosts));
            TotalPostCount = AllPosts.Count;
            CurrentPage = 1;
            UpdatePostsForCurrentPage();
        }
        private Post? _selectedPost;
        public Post? SelectedPost
        {
            get => _selectedPost;
            set
            {
                if (_selectedPost != value)
                {
                    _selectedPost = value;
                    OnPropertyChanged(nameof(SelectedPost));
                }
            }
        }
        public ICommand ChangePageCommand { get; }
        public PostViewModel()
        {
            ChangePageCommand = new RelayCommand(ChangePage);
            LoadPosts();
        }
        private void ChangePage(object? parameter)
        {
            if (parameter is int page && page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
            }
        }
        private void UpdatePageNumbers()
        {
            PageNumbers.Clear();
            for (int i = 1; i <= TotalPages; i++)
            {
                PageNumbers.Add(i);
            }
        }
        private int GetTotalPostCount(MySqlConnection connection)
        {
            string countQuery = "SELECT COUNT(*) FROM posts";
            using (var countCmd = new MySqlCommand(countQuery, connection))
            {
                return Convert.ToInt32(countCmd.ExecuteScalar());
            }
        }
        private List<Post> GetAllPosts(MySqlConnection connection)
        {
            var posts = new List<Post>();
            string query = "SELECT * FROM posts ORDER BY Id DESC";
            using (var cmd = new MySqlCommand(query, connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var post = new Post
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("Title"),
                        Body = reader.GetString("Body"),
                        Type = reader.GetString("Type"),
                        Date = reader.GetDateTime("created_at")
                    };
                    posts.Add(post);
                }
            }
            return posts;
        }
        public void LoadPosts()
        {
            AllPosts.Clear();

            try
            {
                Env.Load();
                string host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
                string port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
                string name = Environment.GetEnvironmentVariable("DB_NAME") ?? "your_db";
                string uid = Environment.GetEnvironmentVariable("DB_UID") ?? "root";
                string pwd = Environment.GetEnvironmentVariable("DB_PWD") ?? "";

                string connStr = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (var connection = new MySqlConnection(connStr))
                {
                    connection.Open();
                    TotalPostCount = GetTotalPostCount(connection);
                    var posts = GetAllPosts(connection);
                    foreach (var post in posts)
                    {
                        AllPosts.Add(post);
                    }
                }
                // Body = bodyParagraphs,
                CurrentPage = 1;
                UpdatePostsForCurrentPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("게시글 로딩 오류: " + ex.Message);
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}