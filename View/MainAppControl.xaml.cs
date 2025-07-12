using System.Windows.Controls;
using user_client.Model;
using user_client.View;
using user_client.ViewModel;

namespace user_client.View
{
    public partial class MainAppControl : System.Windows.Controls.UserControl
    {
        private PostViewModel _viewModel;

        public MainAppControl(PostViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;

            SideMenu.NavigateToListRequested += ShowPostList;
            SideMenu.NavigateToChatRequested += ShowChat;
            ShowPostList(); // 기본 게시판 화면
        }

        private void ShowPostList()
        {
            var postList = new PostListControl(_viewModel);
            postList.NavigateToListRequested += ShowPostList;
            postList.NavigateToChatRequested += ShowChat;

            postList.SelectPostEvent += ShowPostDetail;
            postList.CreateEvent += ShowCreatePost;
            postList.GotoChatEvnt += ShowChat;

            postList.LoadPostsFromDatabase();

            ContentArea.Children.Clear();
            ContentArea.Children.Add(postList);
        }

        private void ShowPostDetail(Post post)
        {
            var detail = new PostDetailControl(post, _viewModel, ShowPostList);
            ContentArea.Children.Clear();
            ContentArea.Children.Add(detail);
        }

        private void ShowCreatePost()
        {
            var create = new CreatePostControl();
            create.PostCreated += post =>
            {
                _viewModel.Posts.Insert(0, post);
                ShowPostDetail(post);
            };
            ContentArea.Children.Clear();
            ContentArea.Children.Add(create);
        }

        private void ShowChat()
        {
            ContentArea.Children.Clear();
            ContentArea.Children.Add(new ChatControl());
        }
    }
}