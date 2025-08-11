using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace user_client.Model
{
    public class ChatClient
    {
        TcpClient client = null;
        Thread receiveMessageThread = null;
        ConcurrentBag<string> sendMessageListToView = null;
        ConcurrentBag<string> receiveMessageListToView = null;
        public event Action<string, string>? receiveMessageEvt;
        public event Action<string>? reloadChatList;
        private string _empId;
        public ChatClient(string empId)
        {
            _empId = empId;
        }

        public void Init()
        {
            sendMessageListToView = new ConcurrentBag<string>();
            receiveMessageListToView = new ConcurrentBag<string>();

            receiveMessageThread = new Thread(ReceiveMessage);
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
                    receiveMessageEvt?.Invoke(sender, msg);
                    reloadChatList?.Invoke(_empId);
                    receiveMessageListToView.Add(string.Format($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Sender: {sender}, Message: {msg}"));
                }
            }
            messageList.Clear();
        }

        public void SendMessage(string message, string roomId)
        {
            if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(message)) {
                Console.WriteLine("Retype Receiver and Message");
                Console.ReadKey();
                return;
            }

            string parsedMessage = string.Format($"{roomId}<{message}>");
            byte[] byteData = Encoding.Default.GetBytes(parsedMessage);

            client.GetStream().Write(byteData, 0, byteData.Length);
            sendMessageListToView.Add(string.Format($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Receiver: {roomId}, Message: {message}"));
            Console.WriteLine("Successful Send Message");
        }

        public void Connect(string empId)
        {

            string parsedName = "%^&" + empId;
            if (parsedName == "%^&") return;

            client = new TcpClient();
            client.Connect("3.27.68.176", 9999);
            //client.Connect("127.0.0.1", 9999);

            byte[] byteData = new byte[1024];
            byteData = Encoding.Default.GetBytes(parsedName);
            client.GetStream().Write(byteData, 0, byteData.Length);

            receiveMessageThread.Start();
        }
    }
}
