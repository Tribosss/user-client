using DotNetEnv;
using MySql.Data.MySqlClient;
using OtpNet;
using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using user_client.Model;

namespace user_client.View
{
    /// <summary>
    /// OTP 입력 화면. 로그인 실패 3회 이상 시 진입.
    /// 인증 성공 시 메인 화면으로 전환.
    /// </summary>
    public partial class TotpControl : System.Windows.Controls.UserControl
    {
        public event Action<UserData>? OtpSuccessEvt; // ✅ OTP 성공 시 메인화면으로 전환하는 이벤트

        private readonly Totp _totp;           // TOTP 인스턴스
        private readonly DispatcherTimer _timer; // 남은 시간 갱신용 타이머
        private DateTime _createdAt;          // 서버에서 OTP 생성된 시간
        private readonly string _userId;      // 로그인 실패한 사용자 ID
        private readonly string _email;       // 로그인 실패한 사용자 이메일

        public TotpControl(string userId, string email)
        {
            InitializeComponent();

            _userId = userId;
            _email = email;

            try
            {
                Env.Load();

                // OTP Secret 생성 및 DB에 저장
                string otpSecret = GenerateOtpSecret(); // 랜덤 Secret 생성
                _totp = new Totp(Base32Encoding.ToBytes(otpSecret), step: 180); // 3분 주기 TOTP 인스턴스 생성

                _createdAt = DateTime.UtcNow; // 생성 시각 기록
                SaveOtpRequestToDatabase(userId, otpSecret, _createdAt); // DB에 otp_requests INSERT

                SendOtpByEmail(email, _totp.ComputeTotp()); // 6자리 OTP 생성 후 Gmail로 전송

                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _timer.Tick += UpdateRemainingTime;
                _timer.Start();
            }
            catch (Exception ex)
            {
                // 로그인 실패 3회 후 이유를 알 수 없게 걍 꺼져서 예외 발생 시 메시지 박스 출력 및 디버깅 로그 남김 추가함
                System.Windows.MessageBox.Show("OTP 초기화 중 오류가 발생했습니다:\n" + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex.ToString());
            }
        }

        private void UpdateRemainingTime(object? sender, EventArgs e)
        {
            int remaining = 180 - (int)(DateTime.UtcNow - _createdAt).TotalSeconds;
            TimeLeftText.Text = $"{Math.Max(0, remaining)}초";
        }

        private void OtpBox_GotFocus(object sender, RoutedEventArgs e)
        {
            OtpPlaceHolder.Visibility = Visibility.Collapsed;
        }

        private void OtpBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OtpBox.Text))
            {
                OtpPlaceHolder.Visibility = Visibility.Visible;
            }
        }
        private void OtpButton_Click(object sender, RoutedEventArgs e)
        {
            string inputCode = OtpBox.Text.Trim();

            // OTP 일치 시
            if (_totp.VerifyTotp(inputCode, out long _, VerificationWindow.RfcSpecifiedNetworkDelay))
            {
                OtpErrorText.Visibility = Visibility.Collapsed;
                _timer.Stop(); // 타이머 중지

                // 로그인 성공 시 DB에서 사용자 정보 로드 후 메인 화면으로 전환
                UserData userData = LoadUserData(_userId);
                OtpSuccessEvt?.Invoke(userData);
            }
            else
            {
                OtpErrorText.Visibility = Visibility.Visible;
            }
        }

        // OTP Secret 랜덤 생성
        private string GenerateOtpSecret()
        {
            byte[] bytes = KeyGeneration.GenerateRandomKey(20); // 160비트 길이의 키 생성
            return Base32Encoding.ToString(bytes); // Base32 문자열로 변환
        }

        // 서버에 OTP 요청 기록 저장
        private void SaveOtpRequestToDatabase(string userId, string secret, DateTime createdAt)
        {
            string host = Environment.GetEnvironmentVariable("DB_HOST");
            string port = Environment.GetEnvironmentVariable("DB_PORT");
            string uid = Environment.GetEnvironmentVariable("DB_UID");
            string pwd = Environment.GetEnvironmentVariable("DB_PWD");
            string db = Environment.GetEnvironmentVariable("DB_NAME");

            string connStr = $"Server={host};Port={port};Database={db};Uid={uid};Pwd={pwd}";

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "INSERT INTO otp_requests (emp_id, otp_secret, created_at) VALUES (@id, @secret, @created)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@secret", secret);
            cmd.Parameters.AddWithValue("@created", createdAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.ExecuteNonQuery(); // INSERT 실행
        }

        // OTP Gmail 전송
        private void SendOtpByEmail(string toEmail, string otpCode)
        {
            string fromEmail = "masterjk1229@gmail.com";
            string fromPwd = "naiv wxil bnrz ijmr"; // Gmail 앱 비밀번호

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPwd),
                EnableSsl = true
            };

            var msg = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Your OTP Code",
                Body = $"인증번호: {otpCode} (3분 이내 입력)"
            };

            smtp.Send(msg); // Gmail로 전송
        }

        //OTP 인증 성공 시 DB에서 유저 정보 로드
        private UserData LoadUserData(string userId)
        {
            string host = Environment.GetEnvironmentVariable("DB_HOST");
            string port = Environment.GetEnvironmentVariable("DB_PORT");
            string uid = Environment.GetEnvironmentVariable("DB_UID");
            string pwd = Environment.GetEnvironmentVariable("DB_PWD");
            string db = Environment.GetEnvironmentVariable("DB_NAME");

            string connStr = $"Server={host};Port={port};Database={db};Uid={uid};Pwd={pwd}";

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "SELECT id, name, role_id, phone, address, age, email FROM employees WHERE id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserData
                {
                    Id = reader.GetInt32("id").ToString(),  // 🔹 INT → string 변환
                    Name = reader.GetString("name"),
                    Position = reader.GetInt32("role_id").ToString(),  // 🔹 role_id도 INT면 변환 필요
                    Phone = reader.IsDBNull("phone") ? null : reader.GetString("phone"),
                    Address = reader.IsDBNull("address") ? null : reader.GetString("address"),
                    Age = reader.IsDBNull("age") ? 0 : reader.GetInt32("age"), // 🔹 int 컬럼은 GetInt32
                    Email = reader.IsDBNull("email") ? null : reader.GetString("email")
                };
            }

            throw new Exception("사용자 정보를 불러올 수 없습니다.");
        }
    }
}


