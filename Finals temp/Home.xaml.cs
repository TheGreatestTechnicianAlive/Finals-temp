using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System.ComponentModel;

namespace Finals_temp
{
    public partial class Home : Window
    {
        DataClasses2DataContext db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString);
        private decimal _cashAmount = 0.00m;
        bool google;
        string _Account;

        private HomeViewModel _viewModel;

        public Home(string account, string name, string email, decimal balance, bool flag)
        {
            InitializeComponent();
            _Account = account;
            google = flag;

            _cashAmount = balance;
            WelcomeText.Text = $"Welcome {name}!\nEmail: {email}";
            UpdateCashAmountDisplay();

            _viewModel = new HomeViewModel(_Account);
            DataContext = _viewModel;
        }

        private void AddCashButton_Click(object sender, RoutedEventArgs e)
        {
            AddBalance addBalanceWindow = new AddBalance(_Account);
            bool? result = addBalanceWindow.ShowDialog();

            if (result == true)
            {
                RefreshBalanceFromDatabase();
            }
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            AddExpense addExpenseWindow = new AddExpense(_Account);
            bool? result = addExpenseWindow.ShowDialog();

            if (result == true)
            {
                decimal expenseAmount = addExpenseWindow.ExpenseAmount;
                _cashAmount -= expenseAmount;
                UpdateCashAmountDisplay();

                try
                {
                    var user1 = db.Users.FirstOrDefault(u => u.UserID == _Account);
                    var user2 = db.GoogleUsers.FirstOrDefault(u => u.GoogleUserID == _Account);

                    if (user1 != null)
                    {
                        user1.Balance = (user1.Balance ?? 0) - (int)expenseAmount;
                    }
                    else if (user2 != null)
                    {
                        user2.Balance = (user2.Balance ?? 0) - (int)expenseAmount;
                    }

                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating balance after expense: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                RefreshBalanceFromDatabase();

                // 🔁 Refresh pie chart after new expense
                _viewModel.RefreshPieChart(_Account);
            }
        }

        private void UpdateCashAmountDisplay()
        {
            CashAmountText.Text = $"₱{_cashAmount:N2}";
        }

        private void RefreshBalanceFromDatabase()
        {
            using (var freshDb = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString))
            {
                var user1 = freshDb.Users.FirstOrDefault(u => u.UserID == _Account);
                var user2 = freshDb.GoogleUsers.FirstOrDefault(u => u.GoogleUserID == _Account);

                int newBalance = 0;
                if (user1 != null)
                    newBalance = user1.Balance ?? 0;
                else if (user2 != null)
                    newBalance = user2.Balance ?? 0;

                _cashAmount = newBalance;
                UpdateCashAmountDisplay();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GoogleLoginDemoTokens");

            if (google)
                Directory.Delete(tokenPath, true);

            MessageBox.Show("Logged out successfully!");
            new MainWindow().Show();
            this.Close();
        }
    }

    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<ISeries> PieSeries { get; set; }

        public HomeViewModel(string account)
        {
            LoadPieChart(account);
        }

        public void RefreshPieChart(string account)
        {
            LoadPieChart(account);
        }

        private void LoadPieChart(string account)
        {
            using (var db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString))
            {
                var expenses = db.ExpenseTables
                    .Where(e => e.Account == account)
                    .GroupBy(e => e.Category_ID)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(e => (decimal?)e.Amount ?? 0)
                    })
                    .ToList();

                var total = expenses.Sum(x => x.TotalAmount);

                var categoryNames = db.CategoryTables
                    .ToDictionary(c => c.Category_ID, c => c.Category_Desc);

                if (total == 0 || !expenses.Any())
                {
                    PieSeries = new List<ISeries>
            {
                new PieSeries<double>
                {
                    Values = new double[] { 100 },
                    Name = "No Data",
                    DataLabelsSize = 14,
                    DataLabelsFormatter = point => $"{point.Model}%"
                }
            };
                }
                else
                {
                    PieSeries = expenses.Select(e =>
                    {
                        double percentage = Math.Round((double)(e.TotalAmount / total * 100), 2);
                        return new PieSeries<double>
                        {
                            Values = new[] { percentage },
                            Name = categoryNames.ContainsKey(e.Category) ? categoryNames[e.Category] : "Unknown",
                            DataLabelsSize = 14,
                            DataLabelsFormatter = point => $"{point.Model}%"
                        };
                    }).ToList();
                }
            }

            OnPropertyChanged(nameof(PieSeries));
        }


        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
