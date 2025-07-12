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
using user_client.Model;
using user_client.ViewModel;

namespace user_client.View.Chat
{
    /// <summary>
    /// Interaction logic for ChatUserListWindow.xaml
    /// </summary>
    public partial class ChatUserListWindow : Window
    {
        private string _currentEmpId;
        public ChatUserListWindow(string empId)
        {
            InitializeComponent();
            _currentEmpId = empId;
            this.DataContext = new ChatUserListViewModel(empId);
        }

        private void User_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2) return;
            Border border = (Border) sender;
            RecentChat user = (RecentChat)border.DataContext;

            UserChattingWindow chattingWindow = new UserChattingWindow(user, _currentEmpId);
            chattingWindow.Show();
        }
    }
}
