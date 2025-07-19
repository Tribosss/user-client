using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using user_client.Model;

namespace user_client.View
{
    /// <summary>
    /// 로그인 화면. ID/PW 입력 및 로그인 시도 처리
    /// </summary>
    public partial class SignInControl : System.Windows.Controls.UserControl
    {
        public event Action? GotoSignUpEvt;
        public event Action<UserData>? SuccessSignInEvt;
        public event Action? RequireOtpEvt; 

        private int _failCount = 0;

        public SignInControl()
        {
            InitializeComponent();
        }

        public SignInControl(Action<UserData> onSuccess, Action onGotoSignUp) : this()
        {
            SuccessSignInEvt += onSuccess;
            GotoSignUpEvt += onGotoSignUp;
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

            UserData? uData = GetUserData(loginId, password);
            if (uData == null)
            {
                _failCount++;
                LoginErrorText.Text = $"사번 또는 비밀번호가 잘못되었습니다. ({_failCount})";
                LoginErrorText.Visibility = Visibility.Visible;

                if (_failCount >= 3)
                {
                    RequireOtpEvt?.Invoke();

                    return;
                }


                return;
            }

            LoginErrorText.Visibility = Visibility.Collapsed;
            SuccessSignInEvt?.Invoke(uData);
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

        private UserData? GetUserData(string empId, string password)
        {
            string query = "select e.id, e.name, r.position, e.phone, e.address, e.age " +
                           "from employees e " +
                           "inner join role r on r.id = e.role_id " +
                           "where e.id = @id and e.password = @password;";

            try
            {
                Env.Load();

                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                string? name = Environment.GetEnvironmentVariable("DB_NAME");

                if (host == null || port == null || uid == null || pwd == null || name == null)
                    throw new Exception("환경변수 누락");

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using MySqlConnection connection = new MySqlConnection(dbConnection);
                connection.Open();

                using MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", empId);
                cmd.Parameters.AddWithValue("@password", password);

                using MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    return new UserData
                    {
                        Id = rdr[0].ToString(),
                        Name = rdr[1].ToString(),
                        Position = rdr[2].ToString() == "ADMIN" ? "관리자" : "사원",
                        Phone = rdr[3].ToString(),
                        Address = rdr[4].ToString(),
                        Age = int.Parse(rdr[5].ToString())
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
