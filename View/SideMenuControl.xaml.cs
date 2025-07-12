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
using System.Windows.Shapes;

namespace user_client.View
{
    /// <summary>
    /// SideMenuControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SideMenuControl : System.Windows.Controls.UserControl
    {
        public event Action? NavigateToListRequested;
        public event Action? NavigateToChatRequested;
        public SideMenuControl()
        {
            InitializeComponent();
        }  

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToListRequested?.Invoke();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigateToChatRequested?.Invoke();
        }

    }
}