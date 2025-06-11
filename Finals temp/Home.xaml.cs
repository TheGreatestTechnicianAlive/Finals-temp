using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

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

                _viewModel.RefreshPieChart(_viewModel.CurrentFilter);
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

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Notifications clicked.");
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Home clicked.");
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Options clicked.");
        }

        private void DateFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedFilter = selectedItem.Content.ToString();

                if (_viewModel != null)
                {
                    _viewModel.ApplyDateFilter(selectedFilter);
                }
            }
        }
    }

    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ISeries> _pieSeries;
        public ObservableCollection<ISeries> PieSeries
        {
            get => _pieSeries;
            set
            {
                _pieSeries = value;
                OnPropertyChanged(nameof(PieSeries));
            }
        }

        public string CurrentFilter { get; private set; } = "Today";
        private readonly string _account;
        public decimal TotalAmount { get; private set; }

        public HomeViewModel(string account)
        {
            _account = account;
            LoadPieChart(CurrentFilter); // Default is "Today"
        }

        public void ApplyDateFilter(string filter)
        {
            CurrentFilter = filter;
            LoadPieChart(filter);
        }

        public void RefreshPieChart(string filter)
        {
            LoadPieChart(filter);
        }

        private void LoadPieChart(string filter)
        {
            using (var db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString))
            {
                var allowedCategories = new[] { "C02", "C03", "C04", "C05" };
                var today = DateTime.Today;

                IQueryable<ExpenseTable> query = db.ExpenseTables
                    .Where(e => e.Account == _account && allowedCategories.Contains(e.Category_ID));

                switch (filter)
                {
                    case "Today":
                        query = query.Where(e => e.Date == today);
                        break;
                    case "This Week":
                        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(e => e.Date >= startOfWeek && e.Date <= today);
                        break;
                    case "This Month":
                        var startOfMonth = new DateTime(today.Year, today.Month, 1);
                        query = query.Where(e => e.Date >= startOfMonth && e.Date <= today);
                        break;
                    case "This Year":
                        var startOfYear = new DateTime(today.Year, 1, 1);
                        query = query.Where(e => e.Date >= startOfYear && e.Date <= today);
                        break;
                    case "All Time":
                    default:
                        // no date filter
                        break;
                }

                var expenses = query
                    .GroupBy(e => e.Category_ID)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(e => (decimal?)e.Amount ?? 0)
                    })
                    .ToList();

                TotalAmount = expenses.Sum(e => e.TotalAmount);

                var categoryNames = db.CategoryTables
                    .Where(c => allowedCategories.Contains(c.Category_ID))
                    .ToDictionary(c => c.Category_ID, c => c.Category_Desc);

                // Fixed color for each category
                var categoryColors = new Dictionary<string, SKColor>
        {
            { "C02", SKColors.Red },       // Transport
            { "C03", SKColors.RoyalBlue},      // Utilities
            { "C04", SKColors.YellowGreen },    // Other
            { "C05", SKColors.MediumBlue }      // Food
        };

                if (TotalAmount == 0 || !expenses.Any())
                {
                    PieSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<double>
                {
                    Values = new double[] { 100 },
                    Name = "No Data",
                    DataLabelsSize = 14,
                    DataLabelsFormatter = point => $"{point.Model}%",
                    Fill = new SolidColorPaint(SKColors.LightGray)
                }
            };
                }
                else
                {
                    PieSeries = new ObservableCollection<ISeries>(
                        expenses.Select(e =>
                        {
                            double percent = Math.Round((double)(e.TotalAmount / TotalAmount * 100), 2);
                            var color = categoryColors.ContainsKey(e.Category) ? categoryColors[e.Category] : SKColors.Gray;

                            return new PieSeries<double>
                            {
                                Values = new[] { percent },
                                Name = categoryNames.ContainsKey(e.Category) ? categoryNames[e.Category] : "Unknown",
                                DataLabelsSize = 14,
                                DataLabelsFormatter = point => $"{point.Model}%",
                                Fill = new SolidColorPaint(color)
                            };
                        })
                    );
                }

                OnPropertyChanged(nameof(TotalAmount));
            }
        }


        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
