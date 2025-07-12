using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using user_client.Model;

namespace user_client.View.Chat
{
    /// <summary>
    /// Interaction logic for UserChattingWindow.xaml
    /// </summary>
    public partial class UserChattingWindow : Window
    {
        private string _roomId;
        public ObservableCollection<ChatLog> ChatLogs { get;  } = new ObservableCollection<ChatLog>();
        public UserChattingWindow(RecentChat recentChat, string empId)
        {
            InitializeComponent();
            _roomId = recentChat.Id;
            TargetName.Text = recentChat.Name;
            this.DataContext = this;
            GetChattingLogs(empId, recentChat.Id);
        }

        private void GetChattingLogs(string empId, string roomId)
        {
            string query = "select m.msg, e.name, r.position, m.created_at, m.sender_id " +
                "from chat_messages m " +
                "inner join employees e on e.id=m.sender_id " +
                "inner join role r on r.id=e.role_id " +
                $"where room_id='{roomId}' " +
                "order by created_at asc";

            string? host, port, uid, pwd, name;
            string dbConnection;
            try
            {
                Env.Load();

                host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) throw new Exception(".env DB_HOST is null");
                port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) throw new Exception(".env DB_PORT is null");
                uid = Environment.GetEnvironmentVariable("DB_UID");
                if (uid == null) throw new Exception(".env DB_UID is null"); ;
                pwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (pwd == null) throw new Exception(".env DB_PWD is null");
                name = Environment.GetEnvironmentVariable("DB_NAME");
                if (name == null) throw new Exception(".env DB_NAME is null");

                dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) return;

                    while (rdr.Read()) {
                        ChatLog log = new ChatLog()
                        {
                            Message = rdr[0].ToString(),
                            Sender = rdr[1].ToString() + $"[{rdr[2].ToString()}]",
                            CreatedAt = DateTime.Parse(rdr[3].ToString()).ToString("HH:mm"),
                            IsMine = rdr[4].ToString() == empId
                        };
                        ChatLogs.Add(log);
                    }

                    connection.Close();
                    return;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
