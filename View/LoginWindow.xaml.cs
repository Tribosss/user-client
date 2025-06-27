using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Windows;

namespace user_client.View
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly string[] keywords = [
            "HelloWorld",
            "Hello",
            "yessss",
        ];

        public LoginWindow()
        {
            InitializeComponent();

            var device = LibPcapLiveDeviceList.Instance[6];
            Console.WriteLine(device.ToString());
            device.Open();
            device.OnPacketArrival += Device_OnPacketArrival;
            device.StartCapture();
        }

        void Device_OnPacketArrival(object s, PacketCapture e)
        {
            byte[]? payload = null;
            byte[] rawBytes = e.GetPacket().Data;
            Packet packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, rawBytes);

            EthernetPacket? ether = packet.Extract<EthernetPacket>();
            if (ether == null) return;
            IPPacket? ip = packet.Extract<IPPacket>();
            if (ip == null) return;

            switch (ip.Protocol) {
                case ProtocolType.Udp:
                    {
                        payload = packet.Extract<UdpPacket>().PayloadData;
                        Console.WriteLine($"Captured: {ip.SourceAddress}:{packet.Extract<UdpPacket>().SourcePort} → {ip.DestinationAddress}:{packet.Extract<UdpPacket>().DestinationPort}");
                        break;
                    }
                case ProtocolType.Tcp: payload = packet.Extract<TcpPacket>().PayloadData; break;
            }
            if (payload == null || payload.Length <= 0) return;
            string text = Encoding.ASCII.GetString(payload);

            foreach (string keyword in keywords)
            {
                if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) continue;

                Console.WriteLine("======================");
                Console.WriteLine($"Detected Keyword: {keyword}");
                Console.WriteLine(text);
                break;
            }
        }
    }
}
