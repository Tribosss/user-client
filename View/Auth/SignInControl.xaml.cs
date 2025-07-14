using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using user_client.Model;

namespace user_client.View
{
    /// <summary>
    /// Interaction logic for SignInControl.xaml
    /// </summary>
    public partial class SignInControl : System.Windows.Controls.UserControl
    {
        public event Action? GotoSignUpEvt;
        public event Action<UserData>? SuccessSignInEvt;

        private int loginFailCount = 0;

        public SignInControl()
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


            bool loginSuccess = true;

            string? host = Environment.GetEnvironmentVariable("DB_HOST");
            string? port = Environment.GetEnvironmentVariable("DB_PORT");
            string? uid = Environment.GetEnvironmentVariable("DB_UID");
            string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
            string? dbname = Environment.GetEnvironmentVariable("DB_NAME");

            string connStr = $"Server={host};Port={port};Database={dbname};Uid={uid};Pwd={pwd}";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    Env.Load();
                    conn.Open();
                    string query = "SELECT password FROM employees WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", loginId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string dbPassword = reader.GetString("password");

                                if (password == dbPassword) // 비밀번호 일치 여부 확인
                                {
                                    loginFailCount = 0;
                                    LoginErrorText.Visibility = Visibility.Collapsed;
                                    System.Windows.MessageBox.Show("로그인 성공!");
                                    UserData uData = GetUserData(loginId);
                                    if (uData == null) throw new Exception("로그인 정보를 찾을 수 없습니다");
                                    SuccessSignInEvt?.Invoke(uData); // 로그인 성공 이벤트 호출
                                }
                                else
                                {
                                    HandleLoginFailure();
                                }
                            }
                            else
                            {
                                LoginErrorText.Text = "존재하지 않는 사번입니다.";
                                LoginErrorText.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoginErrorText.Text = "DB 연결 오류: " + ex.Message;
                    LoginErrorText.Visibility = Visibility.Visible;
                }
            }
        }

        private void HandleLoginFailure()
        {
            loginFailCount++;

            if (loginFailCount >= 3)
            {
                // OTP 화면으로 전환
                OtpControl totpControl = new OtpControl
                {
                    OnSignInSuccess = SuccessSignInEvt,
                    OnGotoSignUp = GotoSignUpEvt
                };


                totpControl.OtpSuccessEvt += () =>
                {
                    System.Windows.MessageBox.Show("OTP 인증 성공!");
                    var parent = this.Parent as System.Windows.Controls.Panel;
                    if (parent != null)
                    {
                        parent.Children.Clear();

                        // 이벤트 핸들러를 넘겨 SignInControl 생성
                        var signIn = new SignInControl(SuccessSignInEvt, GotoSignUpEvt); // ✅
                        parent.Children.Add(signIn);
                    }
                };

                var parent = this.Parent as System.Windows.Controls.Panel;
                parent?.Children.Clear();
                parent?.Children.Add(totpControl);
            }
            else
            {
                LoginErrorText.Text = $"비밀번호가 틀렸습니다. ({loginFailCount}/3)";
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
            GotoSignUpEvt?.Invoke();
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

        private UserData? GetUserData(string empId)
        {
            string query = $"select e.id, e.name, r.position, e.phone, e.address, e.age " +
                $"from employees e " +
                $"inner join role r on r.id=e.role_id " +
                $"where e.id='{empId}';";

            string? host, port, uid, pwd, name;
            string dbConnection;
            UserData uData = new UserData();
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
                    if (rdr == null) return null;

                    while (rdr.Read())
                    {
                        uData = new UserData()
                        {
                            Id = rdr[0].ToString(),
                            Name = rdr[1].ToString(),
                            Position = rdr[2].ToString() == "ADMIN" ? "관리자" : "사원",
                            Phone = rdr[3].ToString(),
                            Address = rdr[4].ToString(),
                            Age = Int32.Parse(rdr[5].ToString()),
                        };
                    }

                    connection.Close();
                    return uData;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
    }
}
