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
        public AddChatRoomWindow(string empId, ChatClient cli)
        {
            InitializeComponent();
            _vm = new AddChatRoomViewModel(empId);
            this.DataContext = _vm;
            _currentEmpId = empId;
            _cli = cli;
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
            if (isExistRoom == 0 || isExistRoom == -1)
            {
                Close();
                return;
            } else if (isExistRoom == 2)
            {
                ChatUserData user = _vm.GetRoomIdByUsersId(_currentEmpId);
                UserChattingWindow chattingWindow = new UserChattingWindow(user, _currentEmpId, _cli);
                chattingWindow.Show();
            }

            _vm.AddChatRoom(_currentEmpId);
            Close();
        }
    }
}
