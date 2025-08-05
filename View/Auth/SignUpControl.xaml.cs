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
    /// Interaction logic for SignUpControl.xaml
    /// </summary>
    public partial class SignUpControl : System.Windows.Controls.UserControl
    {
        public event Action? GotoSignInEvt;

        public SignUpControl()
        {
            InitializeComponent();

            // 플레이스홀더 표시/숨김 이벤트 등록
            SB.GotFocus += (s, e) => SBPlaceholder.Visibility = Visibility.Collapsed;
            SB.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(SB.Text))
                    SBPlaceholder.Visibility = Visibility.Visible;
            };
            SB.TextChanged += (s, e) =>
            {
                SBPlaceholder.Visibility = string.IsNullOrWhiteSpace(SB.Text) ? Visibility.Visible : Visibility.Collapsed;
            };

            PW.GotFocus += (s, e) => PWPlaceholder.Visibility = Visibility.Collapsed;
            PW.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(PW.Password))
                    PWPlaceholder.Visibility = Visibility.Visible;
            };
            PW.PasswordChanged += (s, e) =>
            {
                PWPlaceholder.Visibility = string.IsNullOrWhiteSpace(PW.Password) ? Visibility.Visible : Visibility.Collapsed;
            };

            PW2.GotFocus += (s, e) => PW2Placeholder.Visibility = Visibility.Collapsed;
            PW2.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(PW2.Password))
                    PW2Placeholder.Visibility = Visibility.Visible;
            };
            PW2.PasswordChanged += (s, e) =>
            {
                PW2Placeholder.Visibility = string.IsNullOrWhiteSpace(PW2.Password) ? Visibility.Visible : Visibility.Collapsed;
            };
        }

        private void GoToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            GotoSignInEvt?.Invoke();
        }

        private void ButtonAddName_Click(object sender, RoutedEventArgs e)
        {
            string name = SB.Text;
            string pw = PW.Password;
            string pwConfirm = PW2.Password;

            if (name.Length != 8)
            {
                WarningText.Text = "사원 번호는 8자리 입니다.";
                WarningText.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                WarningText.Text = "사번을 입력하세요.";
                WarningText.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(pw) || string.IsNullOrWhiteSpace(pwConfirm))
            {
                WarningText.Text = "비밀번호 항목은 공백으로 둘 수 없습니다.";
                WarningText.Visibility = Visibility.Visible;
                return;
            }

            if (pw != pwConfirm)
            {
                WarningText.Text = "비밀번호가 일치하지 않습니다.";
                WarningText.Visibility = Visibility.Visible;
                return;
            }

            WarningText.Visibility = Visibility.Collapsed;

            Env.Load();

            string host = Environment.GetEnvironmentVariable("DB_HOST");
            string port = Environment.GetEnvironmentVariable("DB_PORT");
            string uid = Environment.GetEnvironmentVariable("DB_UID");
            string pwd = Environment.GetEnvironmentVariable("DB_PWD");
            string dbName = Environment.GetEnvironmentVariable("DB_NAME");

            if (host == null || port == null || uid == null || pwd == null || dbName == null)
            {
                System.Windows.MessageBox.Show("환경변수(.env) 설정을 확인하세요.");
                return;
            }

            string connectionString = $"Server={host};Port={port};Database={dbName};Uid={uid};Pwd={pwd}";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM employees WHERE id = @id";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@id", name);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        System.Windows.MessageBox.Show("이미 등록된 사번입니다.");
                        return;
                    }

                    byte[] salt = GenerateSalt(16);
                    pw = SHA256Hash(pw, salt);
                    string saltStr = Convert.ToBase64String(salt);
                    string requestedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    string insertQuery = "INSERT INTO signup_requests (temp_emp_id, password, salt, requested_at) VALUES (@id, @pw, @salt, @reqAt)";
                    MySqlCommand cmd = new MySqlCommand(insertQuery, connection);
                    cmd.Parameters.AddWithValue("@id", name);
                    cmd.Parameters.AddWithValue("@pw", pw);
                    cmd.Parameters.AddWithValue("@salt", saltStr);
                    cmd.Parameters.AddWithValue("@reqAt", requestedAt);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        System.Windows.MessageBox.Show("가입 요청이 완료되었습니다!");
                        SB.Clear();
                        PW.Clear();
                        PW2.Clear();
                        GotoSignInEvt?.Invoke();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("가입 요청에 실패했습니다. 다시 시도해주세요.");
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Auth auth = new Auth
            {
                LoginId = name,
                Password = pw
            };
        }

        private byte[] GenerateSalt(int size = 32)
        {
            byte[] salt = new byte[size];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
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
