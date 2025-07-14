using DotNetEnv;
using MySql.Data.MySqlClient;
using PacketDotNet;
using PacketDotNet.Ieee80211;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
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

            // 디바이스 초기화 및 시작
            InitializeDevice();

            SignUpControl control = new SignUpControl();
            control.GotoSignInEvt += HandleGotoSignInControl;
            RootGrid.Children.Add(new SignUpControl());
        }
        private void ShowSignUp()
        {
            var signUp = new SignUpControl();
            signUp.GotoSignInEvt += () =>
            {
                MainContent.Content = new SignInControl();
            };
            MainContent.Content = signUp;
        }

        private void InitializeDevice()
        {
            LibPcapLiveDevice device = LibPcapLiveDeviceList.Instance[6];
            
            Console.WriteLine(device.ToString());
            device.Open();
            device.OnPacketArrival += Device_OnPacketArrival;
            device.StartCapture();
        }

        private void HandleGotoSignInControl()
        {
            ContentArea.Children.Clear();
            SignInControl control = new SignInControl();
            control.GotoSignUpEvt += HandleGotoSignUpControl;
            control.SuccessSignInEvt += InitializePostListControl;
            RootGrid.Children.Add(control);
        }
        private void HandleGotoSignUpControl()
        {
            ContentArea.Children.Clear();
            SignUpControl control = new SignUpControl();
            control.GotoSignInEvt += HandleGotoSignInControl;
            ContentArea.Children.Add(control);
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
                _sharedViewModel.Posts.Insert(0, newPost);
                NavigateToPostDetail(newPost);
            };

            ContentArea.Children.Clear();
            ContentArea.Children.Add(createPostControl);
        }

        private void HandleSelectPost(Post post)
        {
            var postDetailControl = new View.PostDetailControl(post, _sharedViewModel, NavigateToPostList);
            ContentArea.Children.Clear();
            ContentArea.Children.Add(postDetailControl);
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
