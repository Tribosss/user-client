using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace user_client.Model
{
    internal class ChatClient
    {
        TcpClient client = null;
        Thread receiveMessageThread = null;
        ConcurrentBag<string> sendMessageListToView = null;
        ConcurrentBag<string> receiveMessageListToView = null;
        private string name = null;

        public void Init()
        {
            sendMessageListToView = new ConcurrentBag<string>();
            receiveMessageListToView = new ConcurrentBag<string>();

            receiveMessageThread = new Thread(ReceiveMessage);

            while (true)
            {
                Console.WriteLine("=======\n1.Connect Server\n2.Send Message\n3.Check Sent Message\n4.Check Received Message\n=======");

                string key = Console.ReadLine();
                int order = 0;

                if (!int.TryParse(key, out order)) return;

                switch (order)
                {
                    case ChatStaticDefine.CONNECT:
                        {
                            if (client != null)
                            {
                                Console.WriteLine("Already Connected");
                                Console.ReadKey();
                            }
                            else Connect();
                            break;
                        }
                    case ChatStaticDefine.SEND_MESSAGE:
                        {
                            if (client == null)
                            {
                                Console.WriteLine("First Connect to Server");
                                Console.ReadKey();
                            }
                            else SendMessage();

                            break;
                        }
                    case ChatStaticDefine.SEND_MSG_VIEW:
                        {
                            SendMessageView();
                            break;
                        }
                    case ChatStaticDefine.RECEIVE_MSG_VIEW:
                        {
                            ReceiveMessageView();
                            break;
                        }
                        //case ChatStaticDefine.EXIT:
                        //    {
                        //        if (client != null) client.Close();
                        //        receiveMessageThread.Abort();
                        //        return;
                        //    }
                }
            }

        }

        private void ReceiveMessageView()
        {
            if (receiveMessageListToView.Count == 0)
            {
                Console.WriteLine("Received MessageBox is Empty.");
                Console.ReadKey();
                return;
            }

            foreach (var item in receiveMessageListToView)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        private void SendMessageView() {
            if (sendMessageListToView.Count == 0)
            {
                Console.WriteLine("Sent MessageBox is Empty.");
                Console.ReadKey();
                return;
            }

            foreach (var item in sendMessageListToView)
            {
                Console.WriteLine(item);
            }

            Console.ReadKey();
        }

        private void ReceiveMessage()
        {
            string receiveMessage = "";
            List<string> receiveMessageList = new List<string>();

            while(true)
            {
                byte[] receiveByte = new byte[1024];
                client.GetStream().Read(receiveByte, 0, receiveByte.Length);

                receiveMessage = Encoding.Default.GetString(receiveByte);

                string[] receiveMessageArr = receiveMessage.Split(">");
                foreach (var item in receiveMessageArr)
                {
                    if (!item.Contains("<")) continue;
                    if (item.Contains("ADMIN<TEST")) continue;

                    receiveMessageList.Add(item);
                }

                ParsingReceiveMessage(receiveMessageList);
                Thread.Sleep(500);
            }
        }
        
        private void ParsingReceiveMessage(List<string> messageList)
        {
            foreach(var item in messageList)
            {
                string sender = "";
                string msg = "";

                if (item.Contains("<"))
                {
                    string[] splitedMsg = item.Split("<");

                    sender = splitedMsg[0];
                    msg = splitedMsg[1];   

                    if (sender == "ADMIN")
                    {
                        string userList = "";
                        string[] splitedUser = msg.Split("$");

                        foreach (var element in splitedUser)
                        {
                            if (string.IsNullOrEmpty(element)) continue;

                            userList += element + " ";
                        }

                        Console.WriteLine(string.Format($"[Now Connection User] {userList}"));
                        messageList.Clear();
                        return;
                    }

                    Console.WriteLine(string.Format($"[Arrived Message] {sender}: {msg}"));
                    receiveMessageListToView.Add(string.Format($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Sender: {sender}, Message: {msg}"));
                }
            }
            messageList.Clear();
        }

        private void SendMessage()
        {
            string getUserList = string.Format($"{name}<GiveMeUserList>");
            byte[] getUserByte = Encoding.Default.GetBytes(getUserList);
            client.GetStream().Write(getUserByte, 0, getUserByte.Length);

            Console.WriteLine("Type to Receiver");
            string receiver = Console.ReadLine();

            Console.WriteLine("Type to Message");
            string message = Console.ReadLine();

            if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(message)) {
                Console.WriteLine("Retype Receiver and Message");
                Console.ReadKey();
                return;
            }

            string parsedMessage = string.Format($"{receiver}<{message}>");
            byte[] byteData = Encoding.Default.GetBytes(parsedMessage);

            client.GetStream().Write(byteData, 0, byteData.Length);
            sendMessageListToView.Add(string.Format($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Receiver: {receiver}, Message: {message}"));
            Console.WriteLine("Successful Send Message");
            Console.ReadKey();
        }

        private void Connect()
        {
            Console.WriteLine("Type to your name");
            name = Console.ReadLine();

            string parsedName = "%^&" + name;
            if (parsedName == "%^&")
            {
                Console.WriteLine("Is not current name");
                Console.ReadKey();
                return;
            }

            client = new TcpClient();
            client.Connect("3.27.68.176", 9999);

            byte[] byteData = new byte[1024];
            byteData = Encoding.Default.GetBytes(parsedName);
            client.GetStream().Write(byteData, 0, byteData.Length);

            receiveMessageThread.Start();

            Console.WriteLine("Successful Connect to Server");
            Console.ReadKey();
        }
    }
}
