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
        private bool google;
        private string _Account;
        private string _username;
        private string _email;

        private HomeViewModel _viewModel;

        public Home(string account, string name, string email, decimal balance, bool flag)
        {
            InitializeComponent();

            _Account = account;
            _username = name;
            _email = email;
            google = flag;

            _cashAmount = balance;

            UpdateCashAmountDisplay();

            _viewModel = new HomeViewModel(_Account);
            DataContext = _viewModel;

            UpdateCategoryBreakdownUI();

        }


        private void AddCashButton_Click(object sender, RoutedEventArgs e)
        {
            AddBalance addBalanceWindow = new AddBalance(_Account, _username, _email, _cashAmount, google);
            bool? result = addBalanceWindow.ShowDialog();

            if (result == true)
            {
                RefreshBalanceFromDatabase();
            }
            UpdateCategoryBreakdownUI();
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            AddExpense addExpenseWindow = new AddExpense(_Account, _username, _email, _cashAmount, google);
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
                UpdateCategoryBreakdownUI();
            }
        }

        //private void ToggleChartButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (ExpensePieChart.Visibility == Visibility.Visible)
        //    {
        //        ExpensePieChart.Visibility = Visibility.Collapsed;
        //        ToggleChartButton.Content = "Show Chart";
        //    }
        //    else
        //    {
        //        ExpensePieChart.Visibility = Visibility.Visible;
        //        ToggleChartButton.Content = "Hide Chart";
        //    }
        //}

        private void UpdateCategoryBreakdownUI()
        {
            CategoryBreakdownPanel.Children.Clear();

            foreach (var item in _viewModel.CategoryBreakdown)
            {
                var container = new StackPanel
                {
                    Margin = new Thickness(10, 8, 10, 8),
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // Icon
                grid.ColumnDefinitions.Add(new ColumnDefinition()); // Category name
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Amount

                // Icon container
                var iconBorder = new Border
                {
                    Width = 40,
                    Height = 40,
                    CornerRadius = new CornerRadius(20),
                    Background = new SolidColorBrush(Color.FromArgb(30, ((SolidColorBrush)item.BarColor).Color.R, ((SolidColorBrush)item.BarColor).Color.G, ((SolidColorBrush)item.BarColor).Color.B)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = new TextBlock
                    {
                        Text = item.Icon,
                        FontSize = 18,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0),
                    }
                };
                grid.Children.Add(iconBorder);
                Grid.SetColumn(iconBorder, 0);

                // Category + percentage
                var textStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 0, 0)
                };

                textStack.Children.Add(new TextBlock
                {
                    Text = item.CategoryName,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center
                });

                textStack.Children.Add(new TextBlock
                {
                    Text = $"   {item.Percentage}%",
                    FontSize = 13,
                    Foreground = Brushes.Gray,
                    VerticalAlignment = VerticalAlignment.Center
                });

                grid.Children.Add(textStack);
                Grid.SetColumn(textStack, 1);

                // Amount
                var amountText = new TextBlock
                {
                    Text = $"₱{item.Amount:N2}",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center
                };

                grid.Children.Add(amountText);
                Grid.SetColumn(amountText, 2);

                // Progress bar
                var progressBar = new ProgressBar
                {
                    Value = item.Percentage,
                    Maximum = 100,
                    Height = 6,
                    Margin = new Thickness(0, 8, 0, 0),
                    Foreground = item.BarColor,
                    Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                };

                container.Children.Add(grid);
                container.Children.Add(progressBar);

                CategoryBreakdownPanel.Children.Add(container);
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

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Options optionsWindow = new Options(_Account, _username, _email, _cashAmount, google);
            optionsWindow.Show();
            this.Close(); // Or this.Hide(); to keep it in memory
        }


        private void TransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            Transaction transactionWindow = new Transaction(_Account, _username, _email, _cashAmount, google);
            transactionWindow.Show();
            this.Close();
        }

        private void DateFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedFilter = selectedItem.Content.ToString();

                if (_viewModel != null)
                {
                    _viewModel.ApplyDateFilter(selectedFilter);

                    // ⬇️ Call UI refresh after updating ViewModel
                    UpdateCategoryBreakdownUI();
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
        private ObservableCollection<CategoryBreakdownItem> _categoryBreakdown;
        public ObservableCollection<CategoryBreakdownItem> CategoryBreakdown
        {
            get => _categoryBreakdown;
            set
            {
                _categoryBreakdown = value;
                OnPropertyChanged(nameof(CategoryBreakdown));
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
                var breakdownItems = new ObservableCollection<CategoryBreakdownItem>();

                foreach (var e in expenses)
                {
                    string categoryName = categoryNames.ContainsKey(e.Category) ? categoryNames[e.Category] : "Unknown";
                    double percentage = TotalAmount > 0 ? Math.Round((double)(e.TotalAmount / TotalAmount * 100), 2) : 0;
                    SKColor skColor = categoryColors.ContainsKey(e.Category) ? categoryColors[e.Category] : SKColors.Gray;

                    breakdownItems.Add(new CategoryBreakdownItem
                    {
                        CategoryName = categoryName,
                        Icon = GetIconForCategory(e.Category),
                        Amount = e.TotalAmount,
                        Percentage = percentage,
                        BarColor = new SolidColorBrush(Color.FromRgb(skColor.Red, skColor.Green, skColor.Blue))
                    });
                }

                CategoryBreakdown = breakdownItems;

                OnPropertyChanged(nameof(TotalAmount));
            }
        }
        private string GetIconForCategory(string categoryId)
        {
            if (categoryId == "C02") return "💡"; // Utilities
            if (categoryId == "C03") return "🚌"; // Transport
            if (categoryId == "C04") return "🍽"; // Food
            if (categoryId == "C05") return "📦"; // Other
            return "❓";
        }



        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class CategoryBreakdownItem
    {
        public string CategoryName { get; set; }
        public string Icon { get; set; }  // Emoji or icon text
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public Brush BarColor { get; set; }
    }

}
