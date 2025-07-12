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
        public event Action? OtpSuccessEvt;
        public Action? OnSignInSuccess { get; set; }
        public Action? OnGotoSignUp { get; set; }

        // 테스트용 TOTP 키 (실제 환경에서는 사용자별 키를 DB 등에서 불러와야 함)
        private readonly Totp _totp;

        public OtpControl()
        {
            InitializeComponent();

            // Base32로 인코딩된 키 (테스트용, 사용자별로 다르게 해야 안전함)
            string base32Key = "DHY2OX3KEW2UAIAA";
            byte[] secretBytes = Base32Encoding.ToBytes(base32Key);
            _totp = new Totp(secretBytes);

            string currentOtp = _totp.ComputeTotp();
            System.Windows.MessageBox.Show("현재 OTP: " + currentOtp);
        }

        private void OtpButton_Click(object sender, RoutedEventArgs e)
        {
            string inputCode = OtpBox.Text.Trim();

            if (_totp.VerifyTotp(inputCode, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay))
            {
                // 인증 성공
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
                // 인증 실패
                OtpErrorText.Visibility = Visibility.Visible;
            }
        }
        private System.Windows.Controls.Panel? FindParentPanel()
        {
            DependencyObject parent = this;
            while (parent != null && !(parent is System.Windows.Controls.Panel))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as System.Windows.Controls.Panel;
        }
    }
}
