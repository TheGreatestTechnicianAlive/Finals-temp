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

                OpenHomeWindow(payload.Name, payload.Email);

            }
            catch (Exception ex)
            {

                StatusText.Text = "Login failed: " + ex.Message;
            }
        }


        private void OpenHomeWindow(string name, string email)
        {
            var homeWindow = new Home(name, email);
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

        }

        private void signup_btn_Click(object sender, RoutedEventArgs e)
        {
            SignUp signUpWindow = new SignUp();
            signUpWindow.Show();
            this.Close(); // Optional: close the current window if needed
        }
    }
}
