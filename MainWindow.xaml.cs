using DotNetEnv;
using MySql.Data.MySqlClient;
using PacketDotNet;
using PacketDotNet.Ieee80211;
using SharpPcap;
using SharpPcap.LibPcap;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms; // NotifyIcon 클래스 추가
using user_client.Model;
using user_client.View;
using user_client.ViewModel;


namespace user_client
{
    public partial class MainWindow : Window
    {
        // 임시 키워드
        private readonly string[] _keywords = { "HelloWorld", "Hello", "yessss" };
        private readonly PostViewModel _sharedViewModel = new PostViewModel();
        public PostViewModel SharedViewModel => _sharedViewModel;


        public MainWindow()
        {
            
            InitializeComponent();
            // 트레이 초기화
            InitTray();

            // 디바이스 초기화 및 시작
            InitializeDevice();

            var signInView = new SignInControl();
            signInView.GotoSignUpEvt += HandleGotoSignInControl;
            signInView.SuccessSignInEvt += InitializePostListControl;
            ContentArea.Children.Add(signInView);

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

        private void ShowMainApp()
        {
            var mainApp = new MainAppControl(_sharedViewModel);
            MainContent.Content = mainApp;
        }
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPostList();
        }
        public void NavigateToPostList()
        {
            var postListControl = new PostListControl(_sharedViewModel);
            postListControl.SelectPostEvent += NavigateToPostDetail;
            postListControl.CreateEvent += HandleCreateEvent;
            postListControl.GotoChatEvnt += HandleGotoChatView;

            postListControl.LoadPostsFromDatabase();
            ContentArea.Children.Clear();
            ContentArea.Children.Add(postListControl);
        }
        private void NavigateToPostDetail(Model.Post selectedPost)
        {
            var postDetailControl = new View.PostDetailControl(selectedPost, _sharedViewModel, NavigateToPostList);
            ContentArea.Children.Clear();
            ContentArea.Children.Add(postDetailControl);
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
            ContentArea.Children.Add(control);
        }
        private void HandleGotoSignUpControl()
        {
            ContentArea.Children.Clear();
            SignUpControl control = new SignUpControl();
            control.GotoSignInEvt += HandleGotoSignInControl;
            ContentArea.Children.Add(control);
        }

        private void InitializePostListControl()
        {
            ContentArea.Children.Clear();
            PostListControl postListControl = new PostListControl(_sharedViewModel);

            // 이벤트 연결
            postListControl.CreateEvent += HandleCreateEvent;
            postListControl.SelectPostEvent += HandleSelectPost;
            postListControl.GotoChatEvnt += HandleGotoChatView;


            
            ContentArea.Children.Clear();
            ContentArea.Children.Add(postListControl);
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
            ContentArea.Children.Clear();
            ContentArea.Children.Add(new ChatControl());
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

        private void Device_OnPacketArrival(object s, PacketCapture e)
        {
            try { 
                byte[]? payload = null;
                string sourceIp, destIp;
                int sourcePort = 0;
                int destPort = 0;
                string text;

                byte[] rawBytes = e.GetPacket().Data;
                LinkLayers linkLayerType = e.GetPacket().LinkLayerType;

                // 패킷 파싱
                Packet packet = Packet.ParsePacket(linkLayerType, rawBytes);

                EthernetPacket? ether = packet.Extract<EthernetPacket>();
                if (ether == null) return;

                IPPacket? ip = packet.Extract<IPPacket>();
                if (ip == null) return;
                sourceIp = ip.SourceAddress.ToString();
                destIp = ip.DestinationAddress.ToString();

                // TCP/UDP 분류
                switch (ip.Protocol)
                {
                    case ProtocolType.Udp:
                        {
                            UdpPacket udp = packet.Extract<UdpPacket>();
                            if (udp == null) return;
                            sourcePort = udp.SourcePort;
                            destPort = udp.DestinationPort;
                            payload = udp.PayloadData;
                            break;
                        }
                    case ProtocolType.Tcp:
                        {
                            TcpPacket tcp = packet.Extract<TcpPacket>();
                            sourcePort = tcp.SourcePort;
                            destPort = tcp.DestinationPort;
                            payload = tcp.PayloadData;
                            break;
                        }
                }

                if (sourcePort == 3306 || destPort == 3306) return;

                // payload 추출
                if (payload == null || payload.Length <= 0) return;
                text = Encoding.ASCII.GetString(payload);

                // 키워드 탐지
                foreach (string keyword in _keywords)
                {
                    if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) continue;

                    Console.WriteLine("======================");
                    Console.WriteLine($"Detected Keyword: {keyword}");
                    Console.WriteLine($"payload: {text}");

                    string now = DateTime.Now.ToString("yyyy-MM-dd");

                    SuspicionLog detectedLog = new SuspicionLog
                    {
                        Msg = $"Detected Keyword {keyword}",
                        keyword = keyword,
                        EmpId = 1234,
                        SourceIp = sourceIp,
                        DestIp = destIp,
                        SourcePort = sourcePort,
                        DestPort = destPort,
                        DetectedAt = now,
                    };


                    insertSuspicionLog(detectedLog);

                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing packet: {ex.Message}");
            }
        }

        private void insertSuspicionLog(SuspicionLog log)
        {
            try {
                Env.Load();

                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) return;
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) return;
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                if (uid == null) return;
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (pwd == null) return;
                string? name = Environment.GetEnvironmentVariable("DB_NAME");
                if (name == null) return;

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    string query = "insert into suspicion_logs(msg, emp_id, source_ip, dest_ip, source_port, dest_port, keyword, detected_at) ";
                    query += $"values ('{log.Msg}', 1, '{log.SourceIp}', '{log.DestIp}', {log.SourcePort}, {log.DestPort}, '{log.keyword}', '{log.DetectedAt}');";

                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    if (cmd.ExecuteNonQuery() == 1) Console.WriteLine("Success Insert");
                    else Console.WriteLine("Failed Insert");

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
