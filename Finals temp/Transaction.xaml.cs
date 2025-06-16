using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            //UserInfoText.Text = $"Hello {_username}!\nEmail: {_email}";
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            var transactions = db.ExpenseTables
                                 .Where(t => t.Account == _account)
                                 .OrderByDescending(t => t.Date)
                                 .ToList();

            foreach (var trans in transactions)
            {
                var category = db.CategoryTables.FirstOrDefault(c => c.Category_ID == trans.Category_ID)?.Category_Desc ?? "Unknown";

                Border border = new Border
                {
                    Margin = new Thickness(0, 0, 0, 15),
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(10),
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = Colors.Black,
                        Direction = 270,
                        ShadowDepth = 2,
                        Opacity = 0.2
                    }
                };

                StackPanel item = new StackPanel();
                item.Children.Add(new TextBlock
                {
                    Text = $"Date: {trans.Date:yyyy-MM-dd}",
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                });

                item.Children.Add(new TextBlock
                {
                    Text = $"Category: {category}",
                    Foreground = Brushes.Gray
                });

                item.Children.Add(new TextBlock
                {
                    Text = $"Amount: ₱{trans.Amount:N2}",
                    Foreground = Brushes.Black
                });

                if (!string.IsNullOrWhiteSpace(trans.Notes))
                {
                    item.Children.Add(new TextBlock
                    {
                        Text = $"Notes: {trans.Notes}",
                        Foreground = Brushes.Gray
                    });
                }

                border.Child = item;
                TransactionPanel.Children.Add(border);

            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var home = new Home(_account, _username, _email, _balance, _isGoogleUser);
            home.Show();
            this.Close();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var home = new Home(_account, _username, _email, _balance, _isGoogleUser);
            home.Show();
            this.Close();
        }

        private void TransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            // Current window, do nothing or refresh
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Options page is under construction.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
