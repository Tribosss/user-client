using System;
using System.Windows;
using System.Windows.Controls;
using user_client.Model;

namespace user_client.View
{
    public partial class CreatePostControl : System.Windows.Controls.UserControl
    {
        public event Action<Post>? PostCreated;

        public CreatePostControl()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var newPost = new Post
            {
                Title = TitleTextBox.Text,
                Body = BodyTextBox.Text,
                Date = DateTime.Now
            };

            PostCreated?.Invoke(newPost);
        }
    }
}
