using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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

namespace Finals_temp
{
    /// <summary>
    /// Interaction logic for ChangeUsername.xaml
    /// </summary>
    public partial class ChangeUsername : Window
    {
        private string _account;
        private string _username;
        private string _email;
        private decimal _balance;
        private bool _isGoogleUser;
        public ChangeUsername(string account, string username, string email, decimal balance, bool isGoogleUser)
        {
            InitializeComponent();
            _account = account;
            _username = username;
            _email = email;
            _balance = balance;
            _isGoogleUser = isGoogleUser;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = NewUsernameTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(newUsername))
            {
                DataClasses2DataContext db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString);

                if (_isGoogleUser)
                {
                    var googleUser = db.GoogleUsers.FirstOrDefault(u => u.GoogleUserID == _account);
                    if (googleUser != null)
                    {
                        googleUser.Username = newUsername;
                        db.SubmitChanges();

                        MessageBox.Show("Username updated successfully!");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("User not found.");
                    }
                }
                else
                {
                    var user = db.Users.FirstOrDefault(u => u.UserID == _account);
                    if (user != null)
                    {
                        user.Username = newUsername;
                        db.SubmitChanges();

                        MessageBox.Show("Username updated successfully!");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("User not found.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid username.");
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
