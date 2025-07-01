using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Text;
using System.Windows;
using user_client.Model;
using user_client.View;
using System.Windows.Forms; // NotifyIcon 클래스 추가

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

            // PostListControl 초기화
            InitializePostListControl();
            ShowPostList();
            MainContent.Content = new PostListControl();
        }
        public void NavigateTo(System.Windows.Controls.UserControl control)
        {
            MainContent.Content = control;
        }

        private void ShowPostList()
        {
            var postListControl = new PostListControl();
            postListControl.SelectPostEvent += ShowPostDetail;
            MainContent.Content = postListControl;
        }

        private void ShowPostDetail(Post selectedPost)
        {
            var postDetailControl = new PostDetailControl(selectedPost, ShowPostList);
            MainContent.Content = postDetailControl;
        }
        private void InitializeDevice()
        {
            var device = LibPcapLiveDeviceList.Instance[6];
            Console.WriteLine(device.ToString());
            device.Open();
            device.OnPacketArrival += Device_OnPacketArrival;
            device.StartCapture();
        }

        private void InitializePostListControl()
        {
            var postListControl = new PostListControl();

            // 이벤트 연결
            postListControl.CreateEvent += HandleCreateEvent;
            postListControl.SelectPostEvent += HandleSelectPost;

            // RootGrid에 추가
            RootGrid.Children.Clear();
            RootGrid.Children.Add(postListControl);
        }

        private void HandleCreateEvent()
        {
            var createPostControl = new CreatePostControl();

            createPostControl.PostCreated += newPost =>
            {
                RootGrid.Children.Clear();
                RootGrid.Children.Add(new PostDetailControl(newPost));
            };

            RootGrid.Children.Clear();
            RootGrid.Children.Add(createPostControl);
        }



        private void HandleSelectPost(Post post)
        {
            // 선택된 Post로 페이지 전환
            RootGrid.Children.Clear();
            RootGrid.Children.Add(new PostDetailControl(post));
        }

        private void Device_OnPacketArrival(object s, PacketCapture e)
        {
            try
            {
                byte[]? payload = null;
                string text;
                byte[] rawBytes = e.GetPacket().Data;
                LinkLayers linkLayerType = e.GetPacket().LinkLayerType;

                // 패킷 파싱
                Packet packet = Packet.ParsePacket(linkLayerType, rawBytes);
                EthernetPacket? ether = packet.Extract<EthernetPacket>();
                if (ether == null) return;
                IPPacket? ip = packet.Extract<IPPacket>();
                if (ip == null) return;

                // TCP/UDP 분류
                switch (ip.Protocol)
                {
                    case ProtocolType.Udp:
                        payload = packet.Extract<UdpPacket>()?.PayloadData;
                        break;
                    case ProtocolType.Tcp:
                        payload = packet.Extract<TcpPacket>()?.PayloadData;
                        break;
                }

                // payload 추출
                if (payload == null || payload.Length <= 0) return;
                text = Encoding.ASCII.GetString(payload);

                // 키워드 탐지
                foreach (string keyword in _keywords)
                {
                    if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) continue;

                    Console.WriteLine("======================");
                    Console.WriteLine($"Detected Keyword: {keyword}");
                    Console.WriteLine(text);
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing packet: {ex.Message}");
            }
        }

        private void InitTray()
        {
            // 트레이 초기 설정
            NotifyIcon tray = new NotifyIcon
            {
                Icon = Properties.Resources.TribTrayIcon, // 아이콘 리소스 확인 필요
                Visible = true,
                Text = "Tribosss"
            };

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
