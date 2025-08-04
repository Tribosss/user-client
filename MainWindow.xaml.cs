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
        private string _empId;
        public MainWindow()
        {
            InitializeComponent();
            InitTray();
            HandleGotoSignInControl();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            agcli.KillAgent();
        }

        private void HandleGotoSignInControl()
        {
            RootGrid.Children.Clear();
            var signInControl = new SignInControl(SuccessSignIn, HandleGotoSignUpControl);
            signInControl.RequireOtpEvt += HandleGotoOtpControl; // 로그인 3회 실패 시 이벤트 연결
            RootGrid.Children.Add(signInControl);
        }

        // 로그인 실패 3회 시 OTP 인증 화면으로 이동하는 메서드
        private void HandleGotoOtpControl(string userId, string email)
        {
            RootGrid.Children.Clear();
            var otpControl = new TotpControl(userId, email);
            otpControl.OtpSuccessEvt += HandleGotoSignInControl;
            RootGrid.Children.Add(otpControl);
        }

        private void HandleGotoSignUpControl()
        {
            RootGrid.Children.Clear();
            SignUpControl control = new SignUpControl();
            control.GotoSignInEvt += HandleGotoSignInControl;
            RootGrid.Children.Add(control);
        }

        private void SuccessSignIn(UserData uData)
        {
            rbcli = new RabbitClient(uData.Id);
            rbcli.StartAgent(uData.Id);
            _empId = uData.Id;

            PostListControl postListControl = new PostListControl();
            postListControl.CreateEvent += HandleNavigateCreatePost;
            postListControl.SelectPostEvent += HandleNavigatePostDetail;

            SideBarControl snb = new SideBarControl(uData);
            snb.BoardNavigateEvt += HandleNavigatePostListControl;
            snb.PolicyRequestNavigateEvt += () => { };
            snb.ShowChatWindowEvt += HandleShowChatUserList;

            RootGrid.Children.Clear();
            RootGrid.Children.Add(snb);
            RootGrid.Children.Add(postListControl);
        }

        private void HandleShowChatUserList(string empId)
        {
            ChatUserListWindow window = new ChatUserListWindow(empId);
            window.Owner = this;
            window.Show();
        }

        private void HandleNavigatePostListControl()
        {
            var postListControl = new PostListControl();
            postListControl.CreateEvent += HandleNavigateCreatePost;
            postListControl.SelectPostEvent += HandleNavigatePostDetail;

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(postListControl);
        }

        private void HandleNavigateCreatePost(PostViewModel pvm)
        {
            CreatePostControl createPostControl = new CreatePostControl(pvm, _empId);
            createPostControl.PostCreated += HandleNavigatePostDetail;

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(createPostControl);
        }

        private void HandleNavigatePostDetail(Post post, PostViewModel pvm)
        {
            PostDetailControl control = new PostDetailControl(post, pvm, _empId);
            control.NavigatePostList += HandleNavigatePostListControl;
            control.NavigatePostDetail += HandleNavigatePostDetail;
            control.NavigateCreatePost += HandleNavigateCreatePost;
            control.EditRequested += HandleEditPost;

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(control);
        }


        private void HandleEditPost(Post post)
        {
            var createPostControl = new CreatePostControl(post, true);
            createPostControl.PostCreated += HandleNavigatePostDetail;

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(createPostControl);
        }

        private void InitTray()
        {
            // 트레이 초기 설정
            NotifyIcon tray = new NotifyIcon();
            tray.Icon = Properties.Resources.TribTrayIcon;
            tray.Visible = true;
            tray.Text = "Tribosss";

            // 최소화 시 작업표시줄 숨김 & 트레이 표시
            this.StateChanged += (s, e) =>
            {
                if (this.WindowState != WindowState.Minimized) return;
                this.Hide();
                this.ShowInTaskbar = false;
            };

            // 트레이 더블클릭 시 작업표시줄 표시 & 트레이 숨김
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
