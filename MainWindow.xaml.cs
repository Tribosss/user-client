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
using RabbitMQ.Client;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace user_client
{
    public partial class MainWindow : Window
    {
        private Process _agentProc;
        public MainWindow()
        {
            InitializeComponent();

            InitTray();

            HandleGotoSignInControl();
        }
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_agentProc == null || _agentProc.HasExited) return;
            _agentProc.Kill();
        }

        private async Task StartAgentAsync(string empId)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(baseDir, "Agent", "PacketFlowMonitor.exe"),
                Arguments = empId,
                UseShellExecute = false,
            };
            _agentProc = Process.Start(startInfo);


            string queueName = $"client_{empId}";
            string exchangeName = "tribosss";
            string routingKey = "policy.set";
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                ClientProvidedName = $"[{empId}]"
            };
            IConnection conn = await factory.CreateConnectionAsync();
            IChannel channel = await conn.CreateChannelAsync();
            await channel.QueueDeclareAsync(
                queueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false
            );
            await channel.QueueBindAsync(
                queueName,
                exchangeName,
                routingKey,
                null
            );
            await channel.ExchangeDeclareAsync(
                exchangeName,
                ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string msg = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received: <{msg}>");

                await channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false
                );
            };

            string consumerTag = await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );
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
            StartAgentAsync(uData.Id);

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
            PostListControl postListControl = new PostListControl();

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
