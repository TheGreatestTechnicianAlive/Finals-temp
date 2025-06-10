using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.IO;

namespace Finals_temp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataClasses2DataContext db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString);

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
{
            string[] scopes = { "email", "profile" };

    try
    {
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = "364412312501-ke8p99mo287281v8n8rcqrb1t3ab0fh3.apps.googleusercontent.com",
                ClientSecret = "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"
            },
            scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(TokenStorePath(), true)
        );

        if (credential.Token.IsStale)
        {
            await credential.RefreshTokenAsync(CancellationToken.None);
        }

        var payload = await GoogleJsonWebSignature.ValidateAsync(credential.Token.IdToken);

        // Check if the Google user already exists
        var existingUser = db.GoogleUsers.FirstOrDefault(u => u.Email == payload.Email);

        if (existingUser == null)
        {
            // Generate a new custom GoogleUserID (e.g., "G1", "G2", ...)
            string lastID = db.GoogleUsers
                .OrderByDescending(u => u.GoogleUserID)
                .Select(u => u.GoogleUserID)
                .FirstOrDefault();

            int num = 1;
            if (!string.IsNullOrEmpty(lastID) && lastID.Length > 1 && int.TryParse(lastID.Substring(1), out int parsed))
            {
                num = parsed + 1;
            }

            string newGoogleUserID = "G" + num;

            // Create the new Google user
            GoogleUser newUser = new GoogleUser
            {
                GoogleUserID = newGoogleUserID,
                Email = payload.Email,
                Username = payload.Name,
                Balance = 0
            };

            db.GoogleUsers.InsertOnSubmit(newUser);
            db.SubmitChanges();

            existingUser = newUser;
        }

        OpenHomeWindow(existingUser.GoogleUserID, existingUser.Username, existingUser.Email, existingUser.Balance ?? 0, true);
    }
    catch (Exception ex)
    {
        StatusText.Text = "Login failed: " + ex.Message;
    }
}





        private void OpenHomeWindow(string account ,string name, string email, decimal balance ,bool flag)
        {
            var homeWindow = new Home(account, name, email, balance, flag);
            homeWindow.Show();
            this.Close();
        }


        private string TokenStorePath()
        {
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GoogleLoginDemoTokens"
            );
        }

        private void login_btn_Click(object sender, RoutedEventArgs e)
        {
            string email = user_txt.Text.Trim();
            string password = pass_txt.Visibility == Visibility.Visible
                ? pass_txt.Password
                : VisiblePasswordBox.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString))
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

                if (user != null)
                {
                    // Successful login
                    MessageBox.Show("Login successful!", "Welcome", MessageBoxButton.OK, MessageBoxImage.Information);
                    var homeWindow = new Home(
                        user.UserID,
                        user.Username,
                        user.Email,
                        (decimal)(user.Balance ?? 0), // Convert int? to decimal safely
                        false);
                    homeWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowPasswordToggle_Checked(object sender, RoutedEventArgs e)
        {
            VisiblePasswordBox.Text = pass_txt.Password;
            pass_txt.Visibility = Visibility.Collapsed;
            VisiblePasswordBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            pass_txt.Password = VisiblePasswordBox.Text;
            VisiblePasswordBox.Visibility = Visibility.Collapsed;
            pass_txt.Visibility = Visibility.Visible;
        }


        private void signup_btn_Click(object sender, RoutedEventArgs e)
        {
            SignUp signUpWindow = new SignUp();
            signUpWindow.Show();
            this.Close(); // Optional: close the current window if needed
        }
    }
}
