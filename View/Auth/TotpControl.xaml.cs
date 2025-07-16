using OtpNet;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using user_client.View;

namespace user_client.View
{
    /// <summary>
    /// OTP 입력 화면. 로그인 실패 3회 이상 시 진입.
    /// 인증 성공 시 로그인 화면으로 돌아감.
    /// </summary>
    public partial class TotpControl : System.Windows.Controls.UserControl
    {
        public event Action? OtpSuccessEvt;

        private readonly Totp _totp; // TOTP 인스턴스
        private readonly DispatcherTimer _timer; // 남은 시간 갱신용 타이머

        public TotpControl()
        {
            InitializeComponent();

            // 테스트용 Base32 인코딩 키 (사용자별로 다르게 해야 안전함)
            string base32Key = "DHY2OX3KEW2UAIAA";
            byte[] secretBytes = Base32Encoding.ToBytes(base32Key);
            _totp = new Totp(secretBytes, step: 180); // 3분 주기

            // 타이머로 남은 시간 표시
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += UpdateRemainingTime;
            _timer.Start();
        }

        // 남은 시간 텍스트 업데이트
        private void UpdateRemainingTime(object? sender, EventArgs e)
        {
            int remaining = Math.Max(0, _totp.RemainingSeconds());
            TimeLeftText.Text = $"남은 시간: {remaining}초";
        }

        private void OtpButton_Click(object sender, RoutedEventArgs e)
        {
            string inputCode = OtpBox.Text.Trim();

            // OTP 일치 시
            if (_totp.VerifyTotp(inputCode, out long _, VerificationWindow.RfcSpecifiedNetworkDelay))
            {
                OtpErrorText.Visibility = Visibility.Collapsed;
                _timer.Stop(); // 타이머 중지

                // 로그인 화면으로 전환
                if (this.Parent is System.Windows.Controls.Panel parent)
                {
                    parent.Children.Clear();
                    var signInControl = new SignInControl();
                    parent.Children.Add(signInControl);
                }

                OtpSuccessEvt?.Invoke(); // 성공 이벤트 발생
            }
            else
            {
                OtpErrorText.Visibility = Visibility.Visible;
            }
        }
    }
}
