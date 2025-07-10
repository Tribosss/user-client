using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace user_client.View
{
    /// <summary>
    /// Interaction logic for SignInControl.xaml
    /// </summary>
    public partial class SignInControl : System.Windows.Controls.UserControl
    {
        public event Action? GotoSignUpEvt;
        public event Action? SuccessSignInEvt;

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


            bool loginSuccess = (loginId == "12345678" && password == "password123");

            if (loginSuccess)
            {
                LoginErrorText.Visibility = Visibility.Collapsed;


                System.Windows.MessageBox.Show("로그인 성공!");
                SuccessSignInEvt?.Invoke();
            }
            else
            {
                LoginErrorText.Text = "사번 또는 비밀번호가 잘못되었습니다.";
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
