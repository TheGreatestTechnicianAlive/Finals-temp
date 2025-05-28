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
using System.IO;

namespace Finals_temp
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
        public partial class Home : Window
        {
            public Home(string name, string email)
            {
                InitializeComponent();
                WelcomeText.Text = $"Welcome {name}!\nEmail: {email}";
            }

            private string TokenStorePath()
            {
                return System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GoogleLoginDemoTokens"
                );
            }

            private void LogoutButton_Click(object sender, RoutedEventArgs e)
            {
                var tokenPath = TokenStorePath();

                try
                {
                    if (Directory.Exists(tokenPath))
                    {
                        Directory.Delete(tokenPath, true);
                        MessageBox.Show("Logged out successfully!");
                        // Go back to login window
                        new MainWindow().Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No credentials found to delete.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Logout failed: {ex.Message}");
                }
            }
        }

    }
    
