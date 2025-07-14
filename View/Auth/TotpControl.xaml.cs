using OtpNet;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace user_client.View
{
    /// <summary>
    /// OTP 입력 화면. 로그인 실패 3회 이상 시 진입.
    /// 인증 성공 시 로그인 화면으로 돌아감.
    /// </summary>
    public partial class OtpControl : System.Windows.Controls.UserControl
    {
        public event Action? OtpSuccessEvt; // OTP 인증 성공 시 발생
        public Action? OnSignInSuccess { get; set; } // SignInControl로 돌아가기 위한 콜백
        public Action? OnGotoSignUp { get; set; }     // SignUp으로 이동할 때 콜백

        private readonly Totp _totp;

        public OtpControl()
        {
            InitializeComponent();

            // Base32로 인코딩된 키 (테스트용, 실제 환경에서는 사용자별 키 사용)
            string base32Key = "DHY2OX3KEW2UAIAA";
            byte[] secretBytes = Base32Encoding.ToBytes(base32Key);
            _totp = new Totp(secretBytes);

            string currentOtp = _totp.ComputeTotp(); // 현재 OTP 코드 생성
            System.Windows.MessageBox.Show("현재 OTP: " + currentOtp); // 테스트용 출력
        }

        private void OtpButton_Click(object sender, RoutedEventArgs e)
        {
            string inputCode = OtpBox.Text.Trim();

            if (_totp.VerifyTotp(inputCode, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay))
            {
                // 인증 성공 시 로그인 화면으로 복귀
                var parent = FindParentPanel();
                if (parent != null)
                {
                    parent.Children.Clear();
                    var signInControl = new SignInControl(OnSignInSuccess, OnGotoSignUp);
                    parent.Children.Add(signInControl);
                }

                OtpSuccessEvt?.Invoke();
            }
            else
            {
                // 인증 실패 시 오류 메시지 표시
                OtpErrorText.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 부모 Panel을 찾아 반환. 부모가 Grid, StackPanel 등이어야 함.
        /// </summary>
        private System.Windows.Controls.Panel? FindParentPanel()
        {
            DependencyObject parent = this;
            while (parent != null && parent is not System.Windows.Controls.Panel)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as System.Windows.Controls.Panel;
        }
    }
}
