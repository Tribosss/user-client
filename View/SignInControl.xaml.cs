using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient; // DB 연결을 위한 네임스페이스 추가
using DotNetEnv;              // .env에서 DB 환경변수 로드용

namespace user_client.View
{
    public partial class SignInControl : System.Windows.Controls.UserControl
    {
        public event Action? GotoSignUpEvt;
        public event Action? SuccessSignInEvt;

        private int loginFailCount = 0;
        public SignInControl()
        {
            InitializeComponent();
            Env.Load(); // .env 파일 로딩
        }
        public SignInControl(Action? onSuccess, Action? onGotoSignUp) : this()
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

            // DB 접속 정보 읽기
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
                                    SuccessSignInEvt?.Invoke(); // 로그인 성공 이벤트 호출
                                }
                                else
                                {
                                    HandleLoginFailure();
                                    //LoginErrorText.Text = "비밀번호가 틀렸습니다.";
                                    //LoginErrorText.Visibility = Visibility.Visible;
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
                        var signIn = new SignInControl(SuccessSignInEvt, GotoSignUpEvt); //
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
    }
}
