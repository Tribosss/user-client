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
using MySql.Data.MySqlClient;
using System.IO;
using DotNetEnv;

namespace user_client.View
{
    /// <summary>
    /// Interaction logic for PostDetailControl.xaml
    /// </summary>
    public partial class PostDetailControl : System.Windows.Controls.UserControl
    {
        private Action? _navigateBack;
        public PostDetailControl(Post post, Action? navigateBack = null)
        {
            InitializeComponent();
            this.DataContext = post;
            _navigateBack = navigateBack;
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            _navigateBack?.Invoke();
        }
        private void HomeButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
            {
                Console.WriteLine("Error: MainWindow not found.");
                return;
            }

            Console.WriteLine("Navigating to PostListControl...");
            mainWindow.NavigateTo(new PostListControl());
        }



        public void insertQuery()
        {
            try
            {
                Env.Load();

                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) return;
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) return;
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                if (uid == null) return;
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (pwd == null) return;
                string? name = Environment.GetEnvironmentVariable("DB_NAME");
                if (name == null) return;

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO posts (title, body, author, created_at) VALUES ('Sample Title', 'Sample Body', 'Author Name', NOW());";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);

                    if (insertCmd.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Success Insert");
                    }
                    else
                    {
                        Console.WriteLine("Failed Insert");
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void seleteQuery()
        {
            try
            {
                Env.Load();

                string? host = Environment.GetEnvironmentVariable("DB_HOST");
                if (host == null) return;
                string? port = Environment.GetEnvironmentVariable("DB_PORT");
                if (port == null) return;
                string? uid = Environment.GetEnvironmentVariable("DB_UID");
                if (uid == null) return;
                string? pwd = Environment.GetEnvironmentVariable("DB_PWD");
                if (pwd == null) return;
                string? name = Environment.GetEnvironmentVariable("DB_NAME");
                if (name == null) return;

                string dbConnection = $"Server={host};Port={port};Database={name};Uid={uid};Pwd={pwd}";

                using (MySqlConnection connection = new MySqlConnection(dbConnection))
                {
                    connection.Open();

                    string selectQuery = "SELECT id, title, body, author, created_at FROM posts;";
                    MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                    using (MySqlDataReader rdr = selectCmd.ExecuteReader())
                    {
                        string temp = string.Empty;

                        while (rdr.Read())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++)
                            {
                                if (i != rdr.FieldCount - 1)
                                    temp += rdr[i] + ":";
                                else
                                    temp += rdr[i] + "\n";
                            }
                        }

                        Console.WriteLine(temp);
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
