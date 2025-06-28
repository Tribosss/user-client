using System.Windows;
using user_client.Model;

namespace user_client.View
{
    public partial class PostWindow : Window
    {
        public PostWindow()
        {
            InitializeComponent();
        }

        // 데이터를 설정하는 메서드
        public void SetPostDetails(Post postDetails)
        {
            if (postDetails != null)
            {
                PostDetails.Text = $"제목: {postDetails.Title}\n" +
                                   $"작성자: {postDetails.Author}\n" +
                                   $"상태: {postDetails.Status}\n" +
                                   $"작성일: {postDetails.Date}";
            }
        }
    }
}
