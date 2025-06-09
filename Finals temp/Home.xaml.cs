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
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System.ComponentModel;

namespace Finals_temp
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        DataClasses2DataContext db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString);
        private decimal _cashAmount = 0.00m;
        bool google;
        string _Account;

        public Home(string account, string name, string email, decimal balance, bool flag)
        {
            InitializeComponent();
            DataContext = new HomeViewModel();

            _cashAmount = balance; // Set the balance
            WelcomeText.Text = $"Welcome {name}!\nEmail: {email}";

            _Account = account;
            google = flag;

            UpdateCashAmountDisplay();
        }


        private void AddCashButton_Click(object sender, RoutedEventArgs e)
        {
            _cashAmount += 500;
            UpdateCashAmountDisplay();

            try
            {
                // Find the user by Account (assuming _Account holds UserID or Username)
                var user1 = db.Users.FirstOrDefault(u => u.UserID == _Account);

                var user2 = db.GoogleUsers.FirstOrDefault(u => u.GoogleUserID == _Account);
                if (user1 != null)
                {
                    user1.Balance += 500; // Add 500 to existing balance
                    db.SubmitChanges();
                }
                else if (user2 != null)
                {
                    user2.Balance += 500; // Add 500 to existing balance
                    db.SubmitChanges();
                }
                else
                {
                    MessageBox.Show("User not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating balance: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateCashAmountDisplay()
        {
            CashAmountText.Text = $"₱{_cashAmount:N2}";
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var tokenPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GoogleLoginDemoTokens"
            );

            if(google)
                Directory.Delete(tokenPath, true);

            MessageBox.Show("Logged out successfully!");
            new MainWindow().Show();
            this.Close();

            //try
            //{
            //    if (Directory.Exists(tokenPath))
            //    {
            //        Directory.Delete(tokenPath, true);
            //        MessageBox.Show("Logged out successfully!");
            //        // Go back to login window
            //        new MainWindow().Show();
            //        this.Close();
            //    }
            //    else
            //    {
            //        MessageBox.Show("No credentials found to delete.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Logout failed: {ex.Message}");
            //}
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            AddExpense addExpenseWindow = new AddExpense(_Account);
            addExpenseWindow.Show(); // use ShowDialog() for modal window
        }

    }


    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<ISeries> PieSeries { get; set; }

        public HomeViewModel()
        {
            PieSeries = new ISeries[]
            {
            new PieSeries<double> { Values = new double[] { 10 }, Name = "Utilities" },
            new PieSeries<double> { Values = new double[] { 20 }, Name = "Transport" },
            new PieSeries<double> { Values = new double[] { 30 }, Name = "Food" },
            new PieSeries<double> { Values = new double[] { 40 }, Name = "Other" }
            };
        }
    }
}

