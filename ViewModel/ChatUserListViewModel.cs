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
            string query = "select r.id, e.name, role.position, msg.msg, msg.created_at " +
                "from chat_rooms r " +
                "inner join chat_members member on member.room_id=r.id " +
                "inner join chat_messages msg on msg.room_id=r.id " +
                "inner join employees e on e.id=member.emp_id " +
                "inner join role on role.id=e.role_id " +
                "where member.room_id in (" +
                $"   select room_id from chat_members where emp_id='{empId}'" +
                ") and member.emp_id!='12345678' and msg.created_at=(" +
                "   select MAX(created_at)" +
                "   from chat_messages" +
                "   where room_id=r.id" +
                ") " +
                "order by msg.created_at desc;";

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

                    for (int i = 0; rdr.Read(); i++)
                    {
                        // 이전 인텍스와 동일한 ROOM
                        if (i != 0 && RecentChattingUsers[i-1].Id == rdr[0].ToString())
                        {
                            RecentChattingUsers[i - 1].Name += rdr[1].ToString() + $"[{rdr[2].ToString()}],";
                            i--;
                        }
                        // 이전 인덱스와 다른 ROOM or idx = 0
                        else
                        {
                            if (i != 0) RecentChattingUsers[i - 1].Name.TrimEnd(',');
                            RecentChat recentChat = new RecentChat()
                            {
                                Id = rdr[0].ToString(),
                                Name = rdr[1].ToString() + $"[{rdr[2].ToString()}], ",
                                RecentChattingLog = rdr[3].ToString(),
                                SentAt = rdr[4].ToString(),
                            };
                            RecentChattingUsers.Add(recentChat);
                        }
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
