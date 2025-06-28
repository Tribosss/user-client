using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Text;
using System.Windows;

namespace user_client.View
{
    public partial class LoginWindow : Window
    {
        // 임시 키워드
        private readonly string[] _keywords = [
            "HelloWorld",
            "Hello",
            "yessss",
        ];

        public LoginWindow()
        {
            InitializeComponent();
            
            // 트레이 초기화
            InitTray();
            
            // 디바이스 선택 및 열기
            var device = LibPcapLiveDeviceList.Instance[6];
            Console.WriteLine(device.ToString());
            device.Open();
            device.OnPacketArrival += Device_OnPacketArrival;
            device.StartCapture();
        }

        void Device_OnPacketArrival(object s, PacketCapture e)
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
            switch (ip.Protocol) {
                case ProtocolType.Udp:
                    {
                        payload = packet.Extract<UdpPacket>().PayloadData;
                        break;
                    }
                case ProtocolType.Tcp: payload = packet.Extract<TcpPacket>().PayloadData; break;
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
            tray.DoubleClick += delegate (object? s, EventArgs e)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.ShowInTaskbar = true;
                tray.Visible = false;
            };
        }
    }
}
