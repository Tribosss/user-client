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
using user_client.ViewModel;

namespace user_client.View.Chat
{
    /// <summary>
    /// Interaction logic for ChatUserListWindow.xaml
    /// </summary>
    public partial class ChatUserListWindow : Window
    {
        public ChatUserListWindow(string empId)
        {
            InitializeComponent();
            this.DataContext = new ChatUserListViewModel(empId);
        }
    }
}
