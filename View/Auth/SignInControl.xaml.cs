using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
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
        public event Action<string, string>? RequireOtpEvt; // 로그인 실패 3회 시 사용자 정보 전달용 이벤트

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
                    string? email = GetEmailByUserId(loginId); // 사용자 이메일 조회
                    if (!string.IsNullOrEmpty(email))
                    {
                        RequireOtpEvt?.Invoke(loginId, email); // OTP 인증 화면 전환 요청
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("등록된 이메일이 없습니다.");
                    }

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

        private string GetSalt(string empId)
        {
            string query = $@"select salt from employees where id='{empId}';";

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
                
                using MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    return rdr[0].ToString();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return null;

        }

        private UserData? GetUserData(string empId, string password)
        {
            string salt = GetSalt(empId);
            if (salt == null) return null;

            byte[] saltBytes = Convert.FromBase64String(salt);
            string hashedPassword = SHA256Hash(password, saltBytes);
            string query = "select e.id, e.name, r.position, e.phone, e.address, e.age, e.role_id " +
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
                cmd.Parameters.AddWithValue("@password", hashedPassword);

                using MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    int roleId = Convert.ToInt32(rdr[6]);
                    Console.WriteLine($"roleId from DB: {roleId}");
                    string positionText = roleId == 1 ? "관리자" : "사원";

                    return new UserData
                    {
                        Id = rdr[0].ToString(),
                        Name = !rdr.IsDBNull(1) ? rdr[1].ToString() : null,
                        Position = positionText,
                        Phone = !rdr.IsDBNull(3) ? rdr[3].ToString() : null,
                        Address = !rdr.IsDBNull(4) ? rdr[4].ToString() : null,
                        Age = !rdr.IsDBNull(5) ? int.Parse(rdr[5].ToString()) : null
                    };
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        // 사용자 ID로 이메일 조회 (OTP 전송용)
        private string? GetEmailByUserId(string userId)
        {
            string query = "SELECT email FROM employees WHERE id = @id";

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
                cmd.Parameters.AddWithValue("@id", userId);

                using MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    return rdr[0].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private string SHA256Hash(string rawData, byte[] salt)
        {
            byte[] data = Encoding.UTF8.GetBytes(rawData);
            byte[] dataWithSalt = new byte[data.Length + salt.Length];
            Buffer.BlockCopy(salt, 0, dataWithSalt, 0, salt.Length);
            Buffer.BlockCopy(data, 0, dataWithSalt, salt.Length, data.Length);

            SHA256 sha = SHA256.Create();
            byte[] hashBytes = sha.ComputeHash(dataWithSalt);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
