using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace user_client.View
{
    public partial class ChatControl : System.Windows.Controls.UserControl
    {
        TcpClient client = null;
        public ChatControl()
        {
            InitializeComponent();

            while (true) {
                Console.WriteLine("========\n1.Connect Server\n2.Send Msg\n========");

                string key = Console.ReadLine();
                int order = 0;

                if (!int.TryParse(key, out order))
                {
                    Console.Clear();
                    return;
                }

                switch (order)
                {
                    case 1:
                        {
                            if (client != null)
                            {
                                Console.WriteLine("Already Connected at Server");
                                Console.ReadKey();
                            }
                            else ConnectChatServer();

                            break;
                        }
                    case 2:
                        {
                            if (client == null)
                            {
                                Console.WriteLine("First need to Connect at Server");
                                Console.ReadKey();
                            }
                            else SendMSgAtServer();

                            break;
                        }
                }
                Console.Clear();
            }
        }

        private void ConnectChatServer()
        {
            client = new TcpClient();
            client.Connect("127.0.0.2", 9999);
            Console.WriteLine("Connected Server");
            Console.ReadKey();
        }
        
        private void SendMSgAtServer()
        {
            Console.WriteLine("Type to Message");

            string message = Console.ReadLine();
            byte[] byteData = new byte[message.Length];
            byteData = Encoding.Default.GetBytes(message);

            client.GetStream().Write(byteData, 0, byteData.Length);
            Console.WriteLine("Successful Send Message");
            Console.ReadKey();
        }   
    }
}
