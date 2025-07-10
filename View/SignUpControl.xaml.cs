using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        }

        private void GoToLogin_Click(object sender, MouseButtonEventArgs e) // 로그인 화면으로 돌아가게 해주는 이벤트 핸들러
        {
            GotoSignInEvt?.Invoke();
        }

        private void ButtonAddName_Click(object sender, RoutedEventArgs e) // 버튼이 클릭되었을 때 실행되는 이벤트 처리기 메서드
        {
            string name = SB.Text; // 사번 입력창의 텍스트를 name 변수에 저장
            string pw = PW.Password; // 비밀번호
            string pwConfirm = PW2.Password; // 비밀번호 확인

            if (name.Length != 8) // 사원번호가 8자리가 아닐 경우 경고창 표시
            {
                WarningText.Text = "사원 번호는 8자리 입니다.";
                WarningText.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(name)) // 사번이 공백일 경우
            {
                WarningText.Text = "사번을 입력하세요.";
                WarningText.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(pw) || string.IsNullOrWhiteSpace(pwConfirm)) // 비밀번호 항목이 공백일 경우
            {
                WarningText.Text = "비밀번호 항목은 공백으로 둘 수 없습니다.";
                WarningText.Visibility = Visibility.Visible;
                return;
            }

            if (pw != pwConfirm) // 비밀번호 불일치 시
            {
                WarningText.Text = "비밀번호가 일치하지 않습니다.";
                WarningText.Visibility = Visibility.Visible;
                return;
            }

            WarningText.Visibility = Visibility.Collapsed; // 경고 메시지 숨김 (모든 조건 충족 시)

            Env.Load(); // .env 환경변수에서 DB 연결 정보 로드

            // 환경변수로부터 접속 정보 읽기
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

                    // 이미 존재하는 사번인지 확인
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

                    // INSERT 쿼리 작성 (DB 테이블명과 칼럼명에 맞게 수정)
                    string insertQuery = "INSERT INTO signup_requests (temp_emp_id, password, salt, requested_at) VALUES (@id, @pw, @salt, @reqAt)";
                    MySqlCommand cmd = new MySqlCommand(insertQuery, connection);
                    cmd.Parameters.AddWithValue("@id", name); // 사용자 사번
                    cmd.Parameters.AddWithValue("@pw", pw); // 사용자 비밀번호
                    cmd.Parameters.AddWithValue("@salt", saltStr); 
                    cmd.Parameters.AddWithValue("@reqAt", requestedAt); 
                    // role_id는 예시로 1(intern)이라고 지정

                    int result = cmd.ExecuteNonQuery(); // 실행 결과 행 수 반환

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

            // Auth 객체 생성 (추후 사용 목적에 따라 저장 가능)
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
