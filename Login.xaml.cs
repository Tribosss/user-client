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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string loginId = LoginIdBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(loginId) || string.IsNullOrWhiteSpace(password))
            {
                LoginErrorText.Text = "사번과 비밀번호를 모두 입력해주세요.";
                LoginErrorText.Visibility = Visibility.Visible;
                return;
            }


            bool loginSuccess = (loginId == "12345678" && password == "password123");

            if (loginSuccess)
            {
                LoginErrorText.Visibility = Visibility.Collapsed;


                System.Windows.MessageBox.Show("로그인 성공!");


                Sign_up main = new Sign_up();
                main.Show();
                this.Close();
            }
            else
            {
                LoginErrorText.Text = "사번 또는 비밀번호가 잘못되었습니다.";
                LoginErrorText.Visibility = Visibility.Visible;
            }
        }


        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }


        private void GoToSignUp_Click(object sender, MouseButtonEventArgs e)
        {
            Sign_up signUp = new Sign_up();
            signUp.Show();
            this.Close();
        }
        private void LoginIdBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginIdPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void LoginIdBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginIdBox.Text))
            {
                LoginIdPlaceholder.Visibility = Visibility.Visible;
            }
        }
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
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

