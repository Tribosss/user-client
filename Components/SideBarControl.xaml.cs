using Sprache;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using user_client.Model;

namespace user_client.Components
{
    /// <summary>
    /// Interaction logic for SideBarControl.xaml
    /// </summary>
    public partial class SideBarControl : System.Windows.Controls.UserControl
    {
        public SideBarControl(UserData userData)
        {
            InitializeComponent();
            _userData = userData;

            Position.Text = $"직급: {_userData.Position}";
            EmpId.Text = $"사번: {_userData.Id}";
        }


        public Action? BoardNavigateEvt;
        public Action? PolicyRequestNavigateEvt;
        public Action<string>? ShowChatWindowEvt;
        public Action<UserData>? ShowClickUserRow;
        public Action? ShowSignUpRequestors;
        private UserData _userData = null;

        private void BoardNav_Click(object sender, RoutedEventArgs e)
        {
            BoardNavigateEvt?.Invoke();
        }
        private void ChattingNav_Click(object sender, RoutedEventArgs e)
        {
            ShowChatWindowEvt?.Invoke(_userData.Id);
        }
        private void PolicyRequestNav_Click(object sender, RoutedEventArgs e) { }

    }
}
