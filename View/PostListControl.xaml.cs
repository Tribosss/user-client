using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using user_client.ViewModel;

namespace user_client.View
{
    public partial class PostListControl : System.Windows.Controls.UserControl
    {
        private readonly PostViewModel _viewModel;
        public event Action<Post>? SelectPostEvent;
        public PostListControl()
        {
            InitializeComponent();

            _viewModel = this.DataContext as PostViewModel ?? new PostViewModel();
            this.DataContext = _viewModel;

        }

        private void PostList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            Post? selectedPost = _viewModel.SelectedPost;
            if (selectedPost == null) return;
            SelectPostEvent?.Invoke(selectedPost);
        }
    }
}
