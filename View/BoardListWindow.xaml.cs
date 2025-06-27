using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using user_client.Model;

namespace user_client.View
{
    /// <summary>
    /// Interaction logic for BoardListWindow.xaml
    /// </summary>
    public partial class BoardListWindow : Window
    {
        // 게시글 데이터 컬렉션
        public ObservableCollection<Post> Posts { get; set; }
        public BoardListWindow()
        {
            InitializeComponent();

            // 데이터 바인딩을 위한 설정
            Posts = new ObservableCollection<Post>
            {
                new Post { Status = "공지", Title = "환영합니다", Author = "관리자", Date = DateTime.Now.ToString("yyyy.MM.dd") },
                new Post { Status = "공지", Title = "업데이트 안내", Author = "관리자", Date = DateTime.Now.ToString("yyyy.MM.dd") },
                new Post { Status = "질문", Title = "WPF 데이터 바인딩", Author = "사용자1", Date = DateTime.Now.ToString("yyyy.MM.dd") },
                new Post { Status = "일반", Title = "XAML 디자인 문의", Author = "사용자2", Date = DateTime.Now.ToString("yyyy.MM.dd") }
            };

            // DataGrid에 데이터 바인딩
            PostGrid.ItemsSource = Posts;
        }
    }
}
