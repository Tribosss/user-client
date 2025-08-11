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
    public partial class ChatUserListWindow : Window
    {
        private string _currentEmpId;
        private ChatClient _cli { get; set; }
        private ChatUserListViewModel _cvm { get; set; }
        public ChatUserListWindow(string empId)
        {
            InitializeComponent();
            _cli = new ChatClient(empId);
            _cli.Init();
            _cli.Connect(empId);
            _currentEmpId = empId;
            _cvm = new ChatUserListViewModel(empId, this.Dispatcher);
            _cli.reloadChatList += _cvm.LoadChatUserList;
            this.DataContext = _cvm;
        }

        private void User_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2) return;
            Border border = (Border) sender;
            ChatUserData user = (ChatUserData)border.DataContext;

            UserChattingWindow chattingWindow = new UserChattingWindow(user, _currentEmpId, _cli);
            chattingWindow.Show();
        }

        private void AddChatRoomButton_Click(object sender, RoutedEventArgs e)
        {
            AddChatRoomWindow chattingWindow = new AddChatRoomWindow(_currentEmpId, _cli, _cvm);
            chattingWindow.ShowDialog();
            _cvm.LoadChatUserList(_currentEmpId);
        }
    }
}
