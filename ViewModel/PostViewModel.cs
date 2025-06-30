    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using user_client.Model;

    namespace user_client.ViewModel
    {
        class PostViewModel
        {
            public ObservableCollection<Post> Posts { get; set; } = new ObservableCollection<Post>{
                    new Post { Status = "공지", Title = "환영합니다", Author = "관리자", Date = DateTime.Now.ToString("yyyy.MM.dd") },
                    new Post { Status = "공지", Title = "업데이트 안내", Author = "관리자", Date = DateTime.Now.ToString("yyyy.MM.dd") },
                    new Post { Status = "질문", Title = "WPF 데이터 바인딩", Author = "사용자1", Date = DateTime.Now.ToString("yyyy.MM.dd") },
                    new Post { Status = "일반", Title = "XAML 디자인 문의", Author = "사용자2", Date = DateTime.Now.ToString("yyyy.MM.dd") }
            };
            private Post? _selectedPost;
            public Post? SelectedPost
            {
                get => _selectedPost;
                set {
                    if (_selectedPost == value) return;

                    _selectedPost = value;
                    if (_selectedPost == null) return;
                }
            }
        }
    }
    