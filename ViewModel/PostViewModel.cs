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

namespace user_client.ViewModel
{
    public class PostViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Post> AllPosts { get; set; } = new ObservableCollection<Post>();
        public ObservableCollection<Post> Posts { get; set; } = new ObservableCollection<Post>();
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

            OnPropertyChanged(nameof(Posts));
            OnPropertyChanged(nameof(TotalPages));
        }
        public void AddPost(Post post)
        {
            AllPosts.Insert(0, post);
            OnPropertyChanged(nameof(AllPosts));
            CurrentPage = TotalPages;
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
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public PostViewModel()
        {
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CurrentPage > 1);
            LoadPosts();
        }
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdatePostsForCurrentPage();
            }
        }
        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdatePostsForCurrentPage();
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
                            AllPosts.Add(post);
                        }
                    }
                }

                CurrentPage = 1;
                UpdatePostsForCurrentPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("게시글 로딩 오류: " + ex.Message);
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}