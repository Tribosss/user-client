using DotNetEnv;
using MySql.Data.MySqlClient;
using PacketDotNet;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using user_client.Components;
using user_client.Model;
using user_client.View;
using user_client.View.Chat;
using user_client.ViewModel;

namespace user_client
{
    public partial class MainWindow : Window
    {
        private Process _agentProc;
        private IConnection _conn;
        private IChannel _channel;
        private string _empId;
        private BlockSiteClient _blockCli;

        public MainWindow()
        {
            InitializeComponent();
            InitTray();
            HandleGotoSignInControl();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            KillAgent();
        }

        private void KillAgent()
        {
            if (_agentProc == null) return;
            _agentProc.Kill();
            _agentProc.Close();
            Console.WriteLine("Killed Agent");
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
            Console.WriteLine("Started Agent");

            _blockCli = new BlockSiteClient();

            if (_conn != null && _channel != null && _conn.IsOpen && _channel.IsOpen) return;
            ConnectRabbitServer();
        }

        private void ParseAdminMessage(string msg)
        {
            string[] msgs = msg.Split("<");
            string policyType = msgs[0];
            string toggle = msgs[1].Split(">")[0];

            switch (policyType)
            {
                case "AGENT":
                    {
                        if (toggle == "OFF")
                        {
                            KillAgent();
                        }
                        else if (toggle == "ON")
                        {
                            StartAgentAsync(_empId);
                        }
                        break;
                    }
                case "BLOCK":
                    {
                        int start1 = msg.IndexOf('<') + 1;
                        int end1 = msg.IndexOf('>', start1);
                        int start2 = msg.IndexOf('<', end1) + 1;
                        int end2 = msg.IndexOf('>', start2);
                        string domain = msg.Substring(start2, end2 - start2);
                        if (toggle == "OFF")
                        {
                            _blockCli.RemoveDomain(domain);
                        } else if (toggle == "ON")
                        {
                            _blockCli.BlockDomain(domain);
                        }
                        break;
                    }
            }
        }

        private async Task ConnectRabbitServer()
        {
            if (_empId == null || string.IsNullOrEmpty(_empId)) return;

            string queueName = $"client_{_empId}";
            string exchangeName = "tribosss";
            string routingKey = "policy.set";
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                ClientProvidedName = $"[{_empId}]"
            };
            _conn = await factory.CreateConnectionAsync();
            _channel = await _conn.CreateChannelAsync();
            await _channel.QueueDeclareAsync(
                queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
            await _channel.QueueBindAsync(
                queueName,
                exchangeName,
                routingKey,
                null
            );
            await _channel.ExchangeDeclareAsync(
                exchangeName,
                ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += ReceivedMessageAtAdmin;

            string consumerTag = await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );
        }

        private async Task ReceivedMessageAtAdmin(object model, BasicDeliverEventArgs ea)
        {
            byte[] body = ea.Body.ToArray();
            string msg = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received: [{msg}]");

            ParseAdminMessage(msg);

            await _channel.BasicAckAsync(
                deliveryTag: ea.DeliveryTag,
                multiple: false
            );
        }

        private void HandleGotoSignInControl()
        {
            RootGrid.Children.Clear();
            var signInControl = new SignInControl(SuccessSignIn, HandleGotoSignUpControl);
            signInControl.RequireOtpEvt += HandleGotoOtpControl; // 로그인 3회 실패 시 이벤트 연결
            RootGrid.Children.Add(signInControl);
        }

        // 로그인 실패 3회 시 OTP 인증 화면으로 이동하는 메서드
        private void HandleGotoOtpControl(string userId, string email)
        {
            RootGrid.Children.Clear();
            var otpControl = new TotpControl(userId, email);
            otpControl.OtpSuccessEvt += HandleGotoSignInControl;
            RootGrid.Children.Add(otpControl);
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
            _empId = uData.Id;
            StartAgentAsync(uData.Id);

            PostListControl postListControl = new PostListControl();
            postListControl.CreateEvent += HandleNavigateCreatePost;
            postListControl.SelectPostEvent += HandleNavigatePostDetail;

            SideBarControl snb = new SideBarControl(uData);
            snb.BoardNavigateEvt += HandleNavigatePostListControl;
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

        private void HandleNavigatePostListControl()
        {
            var postListControl = new PostListControl();
            postListControl.CreateEvent += HandleNavigateCreatePost;
            postListControl.SelectPostEvent += HandleNavigatePostDetail;

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(postListControl);
        }

        private void HandleNavigateCreatePost(PostViewModel pvm)
        {
            CreatePostControl createPostControl = new CreatePostControl(pvm, _empId);
            createPostControl.PostCreated += HandleNavigatePostDetail;

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(createPostControl);
        }

        private void HandleNavigatePostDetail(Post post, PostViewModel pvm)
        {
            PostDetailControl control = new PostDetailControl(post, pvm, _empId);
            control.NavigatePostList += HandleNavigatePostListControl;
            control.NavigatePostDetail += HandleNavigatePostDetail;
            control.NavigateCreatePost += HandleNavigateCreatePost;
            control.EditRequested += HandleEditPost;

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(control);
        }


        private void HandleEditPost(Post post)
        {
            var createPostControl = new CreatePostControl(post, true);
            createPostControl.PostCreated += HandleNavigatePostDetail;

            RootGrid.Children.RemoveAt(1);
            RootGrid.Children.Add(createPostControl);
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
