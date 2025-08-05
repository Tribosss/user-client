using Microsoft.VisualBasic.ApplicationServices;
using System.Windows;
using System.Windows.Controls;
using user_client.Model;
using user_client.ViewModel;

namespace user_client.View.Chat
{
    /// <summary>
    /// Interaction logic for AddChatRoomWindow.xaml
    /// </summary>
    public partial class AddChatRoomWindow : Window
    {
        public AddChatRoomViewModel _vm;
        public ChatClient _cli;
        private string _currentEmpId;
        private ChatUserListViewModel _cvm { get; set; }
        public AddChatRoomWindow(string empId, ChatClient cli, ChatUserListViewModel cvm)
        {
            InitializeComponent();
            _vm = new AddChatRoomViewModel(empId);
            this.DataContext = _vm;
            _currentEmpId = empId;
            _cli = cli;
            _cvm = cvm;
        }

        private void InviteUserButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
            if (button == null) return;

            UserData user = (UserData)button.DataContext;
            if (user == null) return;

            _vm.AddSelectedUser(user);
        }

        private void AddChatRoomButton_Click(object sender, RoutedEventArgs e)
        {
            int isExistRoom = _vm.IsExistRoom(_currentEmpId);
            UserChattingWindow window;
            if (isExistRoom == 0 || isExistRoom == -1)
            {
                Close();
                return;
            } else if (isExistRoom == 2)
            {
                ChatUserData user = _vm.GetRoomIdByUserId(_currentEmpId);
                window = new UserChattingWindow(user, _currentEmpId, _cli);
                window.Show();
                Close();
            }

            _vm.AddChatRoom(_currentEmpId);
            ChatUserData users = _vm.GetRoomIdByUsers(_currentEmpId);
            window = new UserChattingWindow(users, _currentEmpId, _cli);
            window.Show();
            Close();
            _cvm.LoadChatUserList(_currentEmpId);
        }
    }
}
