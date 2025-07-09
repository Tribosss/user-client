using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using user_client.Model;


namespace user_client.ViewModel
    {
        public class PostViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Post> AllPosts { get; set; } = new ObservableCollection<Post>();
        public ObservableCollection<Post> Posts { get; set; } = new ObservableCollection<Post>();

        private int _currentPage = 1;
        private const int PageSize = 15;
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
            AllPosts.Insert(0, post); // 또는 Add(post) 후 정렬해도 됨
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
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    //public ObservableCollection<Post> Posts { get; set; } = new ObservableCollection<Post>{
    //            //new Post { Status = "공지", Title = "환영합니다", Author = "관리자", Date = DateTime.Now, Body = "1234" },
    //            //new Post { Status = "공지", Title = "업데이트 안내", Author = "관리자", Date = DateTime.Now, Body = "1234"  },
    //            //new Post { Status = "질문", Title = "WPF 데이터 바인딩", Author = "사용자1", Date = DateTime.Now, Body = "1234"  },
    //            //new Post { Status = "일반", Title = "XAML 디자인 문의", Author = "사용자2", Date = DateTime.Now, Body = "1234"  }
    //    };
    //    private Post? _selectedPost;
    //    public Post? SelectedPost
    //    {
    //        get => _selectedPost;
    //        set {
    //            if (_selectedPost == value) return;

    //            _selectedPost = value;
    //            if (_selectedPost == null) return;
    //        }
    //    }
    //}
}