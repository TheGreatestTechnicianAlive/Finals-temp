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
        private decimal _cashAmount = 0.00m;

        public Home(string name, string email)
        {
            InitializeComponent();
            DataContext = new HomeViewModel();
            WelcomeText.Text = $"Welcome {name}!\nEmail: {email}";
            UpdateCashAmountDisplay();
        }

        private void AddCashButton_Click(object sender, RoutedEventArgs e)
        {
            _cashAmount += 500;
            UpdateCashAmountDisplay();
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

            Directory.Delete(tokenPath, true);
            MessageBox.Show("Logged out successfully!");
            // Go back to login window
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

