using System;
using System.Windows;
using System.Windows.Input;
using DotNetEnv;
using MySql.Data.MySqlClient;
using WpfApp;

namespace Project
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }


        void insertQuery()
        {
            try
            {
                Env.Load();

                string host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) return;
                string port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) return;
                string uid = Environment.GetEnvironmentVariable("DB_UID");
                if (uid == null) return;
                string pwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (pwd == null) return;
                string name = Environment.GetEnvironmentVariable("DB_NAME");
                if (name == null) return;

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();
                    string insertQuery = "insert into role(position) values('intern');";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);

                    if (insertCmd.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Success Insert");
                    }
                    else
                    {
                        Console.WriteLine("Failed Insert");
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        void selectQuery()
        {
            try
            {
                Env.Load();

                string host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) return;
                string port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) return;
                string uid = Environment.GetEnvironmentVariable("DB_UID");
                if (uid == null) return;
                string pwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (pwd == null) return;
                string name = Environment.GetEnvironmentVariable("DB_NAME");
                if (name == null) return;

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    string selectQuery = "select * from role";
                    MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                    MySqlDataReader rdr = selectCmd.ExecuteReader();

                    string temp = string.Empty;
                    if (rdr == null) return;

                    while (rdr.Read())
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            if (i != rdr.FieldCount - 1)
                                temp += rdr[i] + ":";
                            else if (i == rdr.FieldCount - 1)
                                temp += rdr[i] + "\n";
                        }
                    }
                    Console.WriteLine(temp);

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

