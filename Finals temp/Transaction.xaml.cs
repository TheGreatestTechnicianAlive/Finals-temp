using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Finals_temp
{
    public partial class Transaction : Window
    {
        private string _account;
        private string _username;
        private string _email;
        private decimal _balance;
        private bool _isGoogleUser;

        DataClasses2DataContext db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString);

        public Transaction(string account, string username, string email, decimal balance, bool isGoogleUser)
        {
            InitializeComponent();

            _account = account;
            _username = username;
            _email = email;
            _balance = balance;
            _isGoogleUser = isGoogleUser;

            LoadTransactions();
        }

        private void LoadTransactions()
        {
            TransactionPanel.Children.Clear();

            var transactions = db.ExpenseTables
                                 .Where(t => t.Account == _account)
                                 .OrderByDescending(t => t.Date)
                                 .ToList();

            foreach (var trans in transactions)
            {
                string categoryName = db.CategoryTables.FirstOrDefault(c => c.Category_ID == trans.Category_ID)?.Category_Desc ?? "Unknown";
                string formattedDate = trans.Date.ToString("MMMM dd, yyyy");
                string notes = string.IsNullOrWhiteSpace(trans.Notes) ? "None" : trans.Notes;

                // Container
                var card = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(245, 247, 250)), // Soft light gray-blue
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 12),
                    Cursor = Cursors.Hand,
                    Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        Direction = 270,
                        ShadowDepth = 1,
                        BlurRadius = 4,
                        Opacity = 0.1
                    }
                };

                // Click to view detail
                card.MouseLeftButtonUp += (s, e) =>
                {
                    var detail = new TransactionDetail(trans.Date.ToString("yyyy-MM-dd"), categoryName, trans.Amount, notes);
                    detail.ShowDialog();
                };

                var content = new StackPanel();

                content.Children.Add(new TextBlock
                {
                    Text = $"📅 {formattedDate}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(55, 71, 79)) // Soft dark gray-blue
                });

                content.Children.Add(new TextBlock
                {
                    Text = $"📂 Category: {categoryName}",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(97, 97, 97)) // Medium gray
                });

                content.Children.Add(new TextBlock
                {
                    Text = $"💰 Amount: ₱{trans.Amount:N2}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)) // Muted green
                });

                content.Children.Add(new TextBlock
                {
                    Text = $"📝 Notes: {notes}",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)), // Soft gray
                    TextWrapping = TextWrapping.Wrap
                });

                card.Child = content;
                TransactionPanel.Children.Add(card);
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }

        private void TransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            // Already here
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new Options(_account, _username, _email, _balance, _isGoogleUser);
            optionsWindow.Show();
            Close();
        }

        private void NavigateToHome()
        {
            var home = new Home(_account, _username, _email, _balance, _isGoogleUser);
            home.Show();
            Close();
        }
    }
}
