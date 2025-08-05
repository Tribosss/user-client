using DotNetEnv;
using MySql.Data.MySqlClient;
using PacketDotNet;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using user_client.Components;
using user_client.Model;
using user_client.View;
using user_client.View.Chat;
using user_client.ViewModel;

namespace user_client
{
    public partial class MainWindow : Window
    {
        private AgentClient agcli;
        private RabbitClient rbcli;

        // 현재 로그인한 사용자
        private UserData _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            InitTray();
            HandleGotoSignInControl();

            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // 안전하게 종료
            if (agcli != null)
            {
                agcli.KillAgent();
            }
        }

        // 로그인 화면으로 이동
        private void HandleGotoSignInControl()
        {
            RootGrid.Children.Clear();
            var signInControl = new SignInControl(SuccessSignIn, HandleGotoSignUpControl);
            signInControl.RequireOtpEvt += HandleGotoOtpControl; // 로그인 3회 실패 시 OTP
            RootGrid.Children.Add(signInControl);
        }

        // 로그인 실패 3회 시 OTP 화면으로 이동
        private void HandleGotoOtpControl(string userId, string email)
        {
            RootGrid.Children.Clear();
            var otpControl = new TotpControl(userId, email);

            // OTP 인증 성공 시 메인화면으로 이동
            otpControl.OtpSuccessEvt += (userData) =>
            {
                SuccessSignIn(userData);
            };

            RootGrid.Children.Add(otpControl);
        }

        // 회원가입 화면으로 이동
        private void HandleGotoSignUpControl()
        {
            RootGrid.Children.Clear();
            var control = new SignUpControl();
            control.GotoSignInEvt += HandleGotoSignInControl;
            RootGrid.Children.Add(control);
        }

        // 로그인 성공 시 메인 화면 초기화
        private void SuccessSignIn(UserData uData)
        {
            _currentUser = uData;
            rbcli = new RabbitClient(uData.Id);
            rbcli.StartAgent(uData.Id);

            // AgentClient 생성
            agcli = new AgentClient();

            // 사이드바 & 포스트 리스트 로드
            var postListControl = new PostListControl();
            postListControl.CreateEvent += HandleNavigateCreatePost;
            postListControl.SelectPostEvent += HandleNavigatePostDetail;

            var snb = new SideBarControl(uData);
            snb.BoardNavigateEvt += HandleNavigatePostListControl;
            snb.PolicyRequestNavigateEvt += () => { };
            snb.ShowChatWindowEvt += HandleShowChatUserList;

            RootGrid.Children.Clear();
            RootGrid.Children.Add(snb);
            RootGrid.Children.Add(postListControl);
        }

        // 채팅 유저 목록 창 표시
        private void HandleShowChatUserList(string empId)
        {
            var window = new ChatUserListWindow(empId)
            {
                Owner = this
            };
            window.Show();
        }

        // 게시판 목록 화면으로 이동
        private void HandleNavigatePostListControl()
        {
            var postListControl = new PostListControl();
            postListControl.CreateEvent += HandleNavigateCreatePost;
            postListControl.SelectPostEvent += HandleNavigatePostDetail;

            if (RootGrid.Children.Count > 1)
            {
                RootGrid.Children.RemoveAt(1);
            }
            RootGrid.Children.Add(postListControl);
        }

        // 게시글 작성 화면으로 이동
        private void HandleNavigateCreatePost(PostViewModel pvm)
        {
            var createPostControl = new CreatePostControl(pvm, _currentUser.Id);
            createPostControl.PostCreated += HandleNavigatePostDetail;

            if (RootGrid.Children.Count > 1)
            {
                RootGrid.Children.RemoveAt(1);
            }
            RootGrid.Children.Add(createPostControl);
        }

        // 게시글 상세 화면으로 이동
        private void HandleNavigatePostDetail(Post post, PostViewModel pvm)
        {
            var control = new PostDetailControl(post, pvm, _currentUser.Id);
            control.NavigatePostList += HandleNavigatePostListControl;
            control.NavigatePostDetail += HandleNavigatePostDetail;
            control.NavigateCreatePost += HandleNavigateCreatePost;
            control.EditRequested += HandleEditPost;

            if (RootGrid.Children.Count > 1)
            {
                RootGrid.Children.RemoveAt(1);
            }
            RootGrid.Children.Add(control);
        }

        // 게시글 수정 화면으로 이동
        private void HandleEditPost(Post post)
        {
            var createPostControl = new CreatePostControl(post, true, _currentUser.Id);
            createPostControl.PostCreated += HandleNavigatePostDetail;

            if (RootGrid.Children.Count > 1)
            {
                RootGrid.Children.RemoveAt(1);
            }
            RootGrid.Children.Add(createPostControl);
        }

        // 트레이 아이콘 초기화
        private void InitTray()
        {
            NotifyIcon tray = new NotifyIcon
            {
                Icon = Properties.Resources.TribTrayIcon,
                Visible = true,
                Text = "Tribosss"
            };

            // 최소화 시 트레이로 이동
            this.StateChanged += (s, e) =>
            {
                if (this.WindowState != WindowState.Minimized) return;
                this.Hide();
                this.ShowInTaskbar = false;
            };

            // 트레이 더블클릭 시 복원
            tray.DoubleClick += delegate
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.ShowInTaskbar = true;
                tray.Visible = false;
            };
        }
    }
}
