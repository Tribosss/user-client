using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using user_client.Model;
using ZstdSharp.Unsafe;

namespace user_client.ViewModel
{
    public class AddChatRoomViewModel
    {
        public ObservableCollection<UserData>? StaffList { get; } = new ObservableCollection<UserData>();
        public ObservableCollection<UserData>? AdminList { get; } = new ObservableCollection<UserData>();
        public ObservableCollection<UserData> SelectedUsers { get; } = new ObservableCollection<UserData>();
        public AddChatRoomViewModel(string empId) {
            LoadUserNames(empId);
        }

        public void AddSelectedUser(UserData user)
        {
            if (SelectedUsers.Any(selectedUser => selectedUser.Id == user.Id)) return;
            SelectedUsers.Add(user);
        }

        private void LoadUserNames(string empId)
        {
            string query = $"select e.id, e.name, r.position from employees e inner join role r on r.id=e.role_id where e.id!='{empId}' order by role_id asc";

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

                    while (rdr.Read())
                    {
                        UserData uData = new UserData()
                        {
                            Id = rdr[0].ToString(),
                            Name = $"{rdr[1].ToString()} [{rdr[0].ToString()}]",
                        };
                        if (rdr[2].ToString() == "STAFF")
                        {
                            StaffList.Add(uData);
                        } else
                        {
                            AdminList.Add(uData);
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

        public int IsExistRoom(string currentEmpId)
        {
            // 초대 유저 0 예외 처리
            if (SelectedUsers.Count == 0) return 0;
            // 그룹채팅의 경우 중복채팅방 허용
            if (SelectedUsers.Count > 1) return 1;

            string query = @$"select m.room_id
                from chat_members m 
                where m.emp_id in ('{currentEmpId}', '{SelectedUsers[0].Id}')
                group by m.room_id
                having count(distinct m.emp_id)=2 and (
                    select count(id) 
                    from chat_members cm 
                    where cm.room_id=m.room_id
                )=2;";


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
                    if (rdr == null) return -1;

                    int i = 0;
                    for (; rdr.Read(); i++) { }
                    connection.Close();

                    // 추출된 row가 0 -> 중복된 채팅방이 없으므로 허용
                    if (i == 0) return 1;
                    // 1 이상일 경우 존재하는 채팅방 열기
                    return 2;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return 0;
            }
        }

        public ChatUserData GetRoomIdByUsers(string empId)
        {
            string usersStr = $"'{empId}'";
            foreach (UserData user in SelectedUsers) usersStr += $",'{user.Id}'";
            string query = $@"
                select 
                    room.id, 
                    group_concat(concat(e.name, '[', role.position,']')  SEPARATOR ', ')
                    from chat_rooms room 
                    inner join chat_members m on m.room_id=room.id
                    inner join employees e on e.id=m.emp_id
                    inner join role on role.id=e.role_id
                    where m.emp_id in ({usersStr})
                    group by room.id 
                    having 
                        count(distinct m.emp_id)={SelectedUsers.Count + 1} 
                        and (select count(id) from chat_members where room_id=room.id)={SelectedUsers.Count + 1};";
            string? host, port, dbUid, dbPwd, dbName;
            string dbConnection;
            try
            {
                Env.Load();

                host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) throw new Exception(".env DB_HOST is null");
                port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) throw new Exception(".env DB_PORT is null");
                dbUid = Environment.GetEnvironmentVariable("DB_UID");
                if (dbUid == null) throw new Exception(".env DB_UID is null"); ;
                dbPwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (dbPwd == null) throw new Exception(".env DB_PWD is null");
                dbName = Environment.GetEnvironmentVariable("DB_NAME");
                if (dbName == null) throw new Exception(".env DB_NAME is null");

                dbConnection = $"Server={host};Port={port};Database={dbName};Uid={dbUid};Pwd={dbPwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) return null;
                    if (!rdr.Read()) return null;

                    string roomId = rdr[0].ToString();
                    string name = rdr[1].ToString();
                    ChatUserData user = new ChatUserData()
                    {
                        Id = roomId,
                        Name = name
                    };

                    connection.Close();
                    return user;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public ChatUserData GetRoomIdByUserId(string empId)
        {
            string query = @$"
                select cm.room_id, e.name, r.position
                from chat_members AS cm
                inner join (
                    SELECT room_id
                    FROM chat_members
                    GROUP BY room_id
                    HAVING COUNT(distinct id) = 2              
                    AND SUM(emp_id = '12345678') = 1 
                ) m ON m.room_id = cm.room_id
                and cm.emp_id <> '12345678'
                inner join employees AS e
                    ON e.id = cm.emp_id
                inner join role r on r.id=e.role_id
                where e.id='12341234'
                order by cm.room_id;";

            string? host, port, dbUid, dbPwd, dbName;
            string dbConnection;
            try
            {
                Env.Load();

                host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) throw new Exception(".env DB_HOST is null");
                port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) throw new Exception(".env DB_PORT is null");
                dbUid = Environment.GetEnvironmentVariable("DB_UID");
                if (dbUid == null) throw new Exception(".env DB_UID is null"); ;
                dbPwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (dbPwd == null) throw new Exception(".env DB_PWD is null");
                dbName = Environment.GetEnvironmentVariable("DB_NAME");
                if (dbName == null) throw new Exception(".env DB_NAME is null");

                dbConnection = $"Server={host};Port={port};Database={dbName};Uid={dbUid};Pwd={dbPwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr == null) return null;
                    if (!rdr.Read()) return null;

                    string roomId = rdr[0].ToString();
                    string name = rdr[1].ToString() + $"[{rdr[2].ToString()}]";
                    ChatUserData user = new ChatUserData()
                    {
                        Id = roomId,
                        Name = name
                    };
                    
                    connection.Close();
                    return user;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public void AddChatRoom(string currentEmpId)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string roomId = Guid.NewGuid().ToString();
            string createRoomQuery = $@"insert into chat_rooms(id, created_at) values ('{roomId}', '{now}');";
            string insertMemberQuery = $@"
                insert into chat_members (room_id, emp_id, joined_at)
                values('{roomId}', '{currentEmpId}', '{now}');";

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

                    MySqlCommand createRoomCmd = new MySqlCommand(createRoomQuery, connection);

                    if (createRoomCmd.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Success Insert Room");

                        MySqlCommand insertMemberCmd = new MySqlCommand(insertMemberQuery, connection);
                        if (insertMemberCmd.ExecuteNonQuery() == 1)
                        {
                            Console.WriteLine("Success Insert Logined Member");
                        }
                        else
                        {
                            throw new Exception("Failed Insert Member");
                        }
                        for (int i = 0; i < SelectedUsers.Count; i++)
                        {
                            insertMemberQuery = $@"insert into chat_members (room_id, emp_id, joined_at)
                                values('{roomId}', '{SelectedUsers[i].Id}', '{now}');";
                            insertMemberCmd = new MySqlCommand(insertMemberQuery, connection);
                            if (insertMemberCmd.ExecuteNonQuery() == 1)
                            {
                                Console.WriteLine("Success Insert Member");
                            }
                            else
                            {
                                throw new Exception("Failed Insert Member");
                            }
                        }


                    }
                    else
                    {
                        Console.WriteLine("Failed Insert Room");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
