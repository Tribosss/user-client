using DotNetEnv;
using MySql.Data.MySqlClient;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.IO;
using System.Text;
using System.Windows;
using user_client.Model;
using user_client.View;

namespace user_client
{
    public partial class MainWindow : Window
    {
        // 임시 키워드
        private readonly string[] _keywords = { "HelloWorld", "Hello", "yessss" };

        public MainWindow()
        {
            InitializeComponent();

            // 트레이 초기화
            InitTray();

            SignUpControl control = new SignUpControl();
            control.GotoSignInEvt += HandleGotoSignInControl;
            RootGrid.Children.Add(new SignUpControl());
        }

        private void HandleGotoSignInControl()
        {
            RootGrid.Children.Clear();
            SignInControl control = new SignInControl();
            control.GotoSignUpEvt += HandleGotoSignUpControl;
            control.SuccessSignInEvt += InitializePostListControl;
            RootGrid.Children.Add(control);
        }
        private void HandleGotoSignUpControl()
        {
            RootGrid.Children.Clear();
            SignUpControl control = new SignUpControl();
            control.GotoSignInEvt += HandleGotoSignInControl;
            RootGrid.Children.Add(control);
        }

        private void InitializePostListControl()
        {
            PostListControl postListControl = new PostListControl();

            // 이벤트 연결
            postListControl.CreateEvent += HandleCreateEvent;
            postListControl.SelectPostEvent += HandleSelectPost;
            postListControl.GotoChatEvnt += HandleGotoChatView;
            

            // RootGrid에 추가
            RootGrid.Children.Clear();
            RootGrid.Children.Add(postListControl);
        }

        private void HandleCreateEvent()
        {
            CreatePostControl createPostControl = new CreatePostControl();

            createPostControl.PostCreated += newPost =>
            {
                RootGrid.Children.Clear();
                RootGrid.Children.Add(new PostDetailControl(newPost));
            };

            RootGrid.Children.Clear();
        }

        private void HandleSelectPost(Post post)
        {
            RootGrid.Children.Clear();
            RootGrid.Children.Add(new PostDetailControl(post));
        }
        private void HandleGotoChatView()
        {
            RootGrid.Children.Clear();
            RootGrid.Children.Add(new ChatControl());
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
