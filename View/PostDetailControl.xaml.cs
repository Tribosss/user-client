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

namespace user_client.View
{
    /// <summary>
    /// Interaction logic for PostDetailControl.xaml
    /// </summary>
    public partial class PostDetailControl : System.Windows.Controls.UserControl
    {
        private Action _goBackCallback;
        public PostDetailControl(Post selectedPost, Action goBackCallback )
        {
            _goBackCallback= goBackCallback;
            InitializeComponent();
            this.DataContext = selectedPost;
        }
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _goBackCallback?.Invoke();
        }
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateToPostList(); // ← MainWindow에 이 메서드가 public이어야 함
            }
        }

    }
}
