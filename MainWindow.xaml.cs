using System;
using System.Windows;
using user_client.Model;
using user_client.View;
using user_client.ViewModel;

namespace user_client
{
    public partial class MainWindow : Window
    {
        // 로그인된 사용자 정보 저장
        private UserData _userData;

        // ViewModel 인스턴스 (Post 관련)
        private PostViewModel _postViewModel;

        public MainWindow(UserData userData)
        {
            InitializeComponent();

            _userData = userData;
            _postViewModel = new PostViewModel();

            NavigateToSignIn(); // 앱 시작 시 로그인 화면부터 시작
        }

        // 로그인 화면으로 이동
        public void NavigateToSignIn()
        {
            var signIn = new SignInControl();
            signIn.GotoSignUpEvt += NavigateToSignUp;
            signIn.SuccessSignInEvt += () =>
            {
                _userData = signIn.LoggedInUserData;
                NavigateToPostList();
            };

            ContentArea.Children.Clear();
            ContentArea.Children.Add(signIn);
        }

        // 회원가입 화면으로 이동
        public void NavigateToSignUp()
        {
            var signUp = new SignUpControl();
            signUp.OnGotoSignIn += NavigateToSignIn;

            ContentArea.Children.Clear();
            ContentArea.Children.Add(signUp);
        }

        // 게시글 리스트 화면으로 이동
        public void NavigateToPostList()
        {
            var postList = new PostListControl(_postViewModel, _userData);
            postList.SelectPostEvent += NavigateToPostDetail;
            postList.CreateEvent += NavigateToCreatePost;
            postList.NavigateToListRequested += NavigateToPostList;
            postList.NavigateToChatRequested += NavigateToChat;

            ContentArea.Children.Clear();
            ContentArea.Children.Add(postList);
        }

        // 게시글 상세 화면으로 이동
        public void NavigateToPostDetail(Post selectedPost)
        {
            var postDetail = new PostDetailControl(_postViewModel, selectedPost);
            postDetail.OnBackToList += NavigateToPostList;
            postDetail.OnEditRequested += () =>
            {
                NavigateToEditPost(selectedPost);
            };

            ContentArea.Children.Clear();
            ContentArea.Children.Add(postDetail);
        }

        // 게시글 작성 화면으로 이동
        public void NavigateToCreatePost()
        {
            var create = new CreatePostControl(_postViewModel);
            create.PostCreated += _ =>
            {
                NavigateToPostList();
            };

            ContentArea.Children.Clear();
            ContentArea.Children.Add(create);
        }

        // 게시글 수정 화면으로 이동
        public void NavigateToEditPost(Post post)
        {
            var edit = new CreatePostControl(_postViewModel, post, true);
            edit.PostCreated += _ =>
            {
                NavigateToPostList();
            };

            ContentArea.Children.Clear();
            ContentArea.Children.Add(edit);
        }

        // 채팅 화면으로 이동
        public void NavigateToChat()
        {
            var chat = new ChatControl();

            ContentArea.Children.Clear();
            ContentArea.Children.Add(chat);
        }
    }
}
