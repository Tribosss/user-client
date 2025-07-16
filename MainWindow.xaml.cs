using DotNetEnv;
using MySql.Data.MySqlClient;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private Process _agentProc;
        public PostViewModel SharedViewModel { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            SharedViewModel = new PostViewModel();
            InitTray();

            HandleGotoSignInControl();
        }
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_agentProc == null || _agentProc.HasExited) return;
            _agentProc.Kill();
        }
        private void NavigateToPostList()
        {
            HandlePostListControl(); // 게시판 목록 화면으로 전환
        }
        private void NavigateToPostDetail(Post post)
        {
            var postDetailControl = new PostDetailControl(post, NavigateToPostList);

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(postDetailControl);
        }
        private void StartAgent(string empId)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(baseDir, "Agent", "PacketFlowMonitor.exe"),
                Arguments = empId,
                UseShellExecute = false,
            };
            _agentProc = Process.Start(startInfo);
        }

        private void HandleGotoSignInControl()
        {
            RootGrid.Children.Clear();
            SignInControl control = new SignInControl();
            control.GotoSignUpEvt += HandleGotoSignUpControl;
            control.SuccessSignInEvt += SuccessSignIn;
            RootGrid.Children.Add(control);
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
            StartAgent(uData.Id);

            PostListControl postListControl = new PostListControl();
            postListControl.CreateEvent += HandleCreateEvent;
            postListControl.SelectPostEvent += HandleSelectPost;

            SideBarControl snb = new SideBarControl(uData);
            snb.BoardNavigateEvt += HandlePostListControl;
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

        private void HandlePostListControl()
        {
            var postListControl = new PostListControl
            {
                DataContext = SharedViewModel  // 💡 공통 ViewModel 주입!
            };

            // 이벤트 연결
            postListControl.CreateEvent += HandleCreateEvent;
            postListControl.SelectPostEvent += HandleSelectPost;
            

            // RootGrid에 추가
            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(postListControl);
        }

        private void HandleCreateEvent()
        {
            CreatePostControl createPostControl = new CreatePostControl();

            createPostControl.PostCreated += updatedPost =>
            {
                RootGrid.Children.RemoveAt(1);
                RootGrid.Children.Add(new PostDetailControl(updatedPost, NavigateToPostList));
            };

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(createPostControl);
        }

        private void HandleSelectPost(Post post)
        {
            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(new PostDetailControl(post, NavigateToPostList)); 
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
