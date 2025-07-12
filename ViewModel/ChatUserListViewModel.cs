using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using user_client.Model;

namespace user_client.ViewModel
{

    public class ChatUserListViewModel
    {
        public ObservableCollection<RecentChat> RecentChattingUsers { get; } = new ObservableCollection<RecentChat>();
        private string _currentEmpId;

        public ChatUserListViewModel(string empId)
        {
            _currentEmpId = empId;
            GetChatUserList(empId);
        }
        
        private void GetChatUserList(string empId)
        {
            string query = "select m.sender_id, e.name, r.position, m.msg, m.created_at " +
                "from chat_messages m " +
                "inner join ( " +
                "   select sender_id, max(created_at) as max_created_at " +
                "   from chat_messages " +
                "   where room_id in (" +
                "       select room_id " +
                "       from chat_members " +
                $"       where emp_id='{empId}'" +
                $"   ) and sender_id!='{empId}' " +
                "   group by sender_id" +
                ") t on m.sender_id=t.sender_id and m.created_at=t.max_created_at " +
                "inner join employees e on e.id=m.sender_id " +
                "inner join role r on r.id=e.role_id " +
                "order by m.created_at asc;";

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
                        RecentChat recentChat = new RecentChat()
                        {
                            Id = rdr[0].ToString(),
                            Name = rdr[1].ToString() + $" [{rdr[2].ToString()}]",
                            RecentChattingLog = rdr[3].ToString(),
                            SentAt = rdr[4].ToString(),
                        };
                        RecentChattingUsers.Add(recentChat);
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
