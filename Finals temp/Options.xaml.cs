using System;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Finals_temp
{
    public partial class Options : Window
    {
        private string _account;
        private string _username;
        private string _email;
        private decimal _balance;
        private bool _isGoogleUser;

        public Options(string account, string username, string email, decimal balance, bool isGoogleUser)
        {
            InitializeComponent();
            _account = account;
            _username = username;
            _email = email;
            _balance = balance;
            _isGoogleUser = isGoogleUser;

            WelcomeText.Text = $"Username {username}!\nEmail: {email}";

        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GoogleLoginDemoTokens");

            if (_isGoogleUser && Directory.Exists(tokenPath))
                Directory.Delete(tokenPath, true);

            MessageBox.Show("Logged out successfully!");
            new MainWindow().Show();
            this.Close();
        }

        private void ChangeUsername_Click(object sender, MouseButtonEventArgs e)
        {
            new ChangeUsername(_account, _username, _email, _balance, _isGoogleUser).Show();
        }

        private void ChangePassword_Click(object sender, MouseButtonEventArgs e)
        {
            new ChangePassword(_account, _username, _email, _balance, _isGoogleUser).Show();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var home = new Home(_account, _username, _email, _balance, _isGoogleUser);
            home.Show();
            this.Close();
        }
    }
}
