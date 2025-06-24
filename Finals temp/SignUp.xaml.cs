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

namespace Finals_temp
{
    /// <summary>
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Window
    {
        DataClasses2DataContext db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString);

        public SignUp()
        {
            InitializeComponent();
        }

        private void ShowPasswordToggle_Checked(object sender, RoutedEventArgs e)
        {
            VisiblePasswordBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordBorder.Visibility = Visibility.Collapsed;
            VisiblePasswordBox.Visibility = Visibility.Visible;
            VisiblePasswordBorder.Visibility = Visibility.Visible;
        }

        private void ShowPasswordToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = VisiblePasswordBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordBorder.Visibility = Visibility.Visible;
            VisiblePasswordBox.Visibility = Visibility.Collapsed;
            VisiblePasswordBorder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ShowPasswordToggle.IsChecked == true)
                VisiblePasswordBox.Text = PasswordBox.Password;
        }

        private void VisiblePasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ShowPasswordToggle.IsChecked == true)
                PasswordBox.Password = VisiblePasswordBox.Text;
        }


        private void LoginRedirect_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string id;
            string username = UsernameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = ShowPasswordToggle.IsChecked == true
                              ? VisiblePasswordBox.Text.Trim()
                              : PasswordBox.Password.Trim();

            // Basic validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString))
            {
                // Check if user already exists
                var existingUser = db.Users.FirstOrDefault(u => u.Email == email);
                if (existingUser != null)
                {
                    MessageBox.Show("An account with this email already exists.", "Signup Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Generate new custom UserID (e.g., "U1", "U2", ...)
                string lastID = db.Users
                    .OrderByDescending(u => u.UserID)
                    .Select(u => u.UserID)
                    .FirstOrDefault();

                int num = 1;
                if (!string.IsNullOrEmpty(lastID) && lastID.Length > 1 && int.TryParse(lastID.Substring(1), out int parsed))
                {
                    num = parsed + 1;
                }

                string newUserID = "U" + num;
                id = newUserID;

                // Create and insert new user
                var newUser = new User
                {
                    UserID = newUserID,
                    Username = username,
                    Email = email,
                    Password = password,
                    Balance = 0
                };

                db.Users.InsertOnSubmit(newUser);
                db.SubmitChanges();
            }

            MessageBox.Show("Account created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);


            // Optional: Redirect to login
            //MainWindow login = new MainWindow();
            //login.Show();
            //this.Close();

            // ✅ Open Home window after successful signup
            var home = new Home(id, username, email, 0.00m, false); // balance defaults to 0.00
            home.Show();
            this.Close();
        }
    }
}
