using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Finals_temp
{
    public partial class AddBalance : Window
    {
        private string _account;
        private string _username;
        private string _email;
        private decimal _balance;
        private bool _isGoogleUser;

        private readonly DataClasses2DataContext db;
        private StringBuilder _inputBuilder = new StringBuilder();
        private decimal _allowanceAmount = 0; // Always updated

        public decimal AllowanceAmount => _allowanceAmount;

        public AddBalance(string account, string username, string email, decimal balance, bool isGoogleUser)
        {
            InitializeComponent();
            _account = account;
            _username = username;
            _email = email;
            _balance = balance;
            _isGoogleUser = isGoogleUser;
            db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString);
        }

        private void SaveBalance_Click(object sender, RoutedEventArgs e)
        {
            decimal addedAmount = _allowanceAmount;

            if (addedAmount > 0)
            {
                try
                {
                    var user = db.Users.FirstOrDefault(u => u.UserID == _account);
                    var gUser = db.GoogleUsers.FirstOrDefault(u => u.GoogleUserID == _account);

                    if (user != null)
                    {
                        user.Balance = (user.Balance ?? 0) + (int)addedAmount;
                    }
                    else if (gUser != null)
                    {
                        gUser.Balance = (gUser.Balance ?? 0) + (int)addedAmount;
                    }
                    else
                    {
                        MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Generate new Expense_ID
                    var allIds = db.ExpenseTables.Select(x => x.Expense_ID).ToList();
                    int lastNum = allIds
                        .Where(id => id.StartsWith("E") && int.TryParse(id.Substring(1), out _))
                        .Select(id => int.Parse(id.Substring(1)))
                        .DefaultIfEmpty(0)
                        .Max();
                    string newId = "E" + (lastNum + 1);

                    // Create allowance record in ExpenseTable with Category_ID = "C01"
                    var allowanceExpense = new ExpenseTable
                    {
                        Expense_ID = newId,
                        Account = _account,
                        Category_ID = "C01", // Allowance
                        Date = DateTime.Today,
                        Amount = addedAmount,
                        Notes = NotesTextBox.Text
                    };

                    db.ExpenseTables.InsertOnSubmit(allowanceExpense);

                    db.SubmitChanges();

                    MessageBox.Show($"₱{addedAmount:N2} added to balance!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update balance.\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid positive amount.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string value = btn.Content.ToString();

                if (value == "." && _inputBuilder.ToString().Contains("."))
                    return;

                _inputBuilder.Append(value);

                if (decimal.TryParse(_inputBuilder.ToString(), out decimal val))
                {
                    _allowanceAmount = val;
                    AmountTextBox.Text = "₱" + val.ToString("N2");
                }
                else
                {
                    _allowanceAmount = 0;
                    AmountTextBox.Text = "₱0.00";
                }
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_inputBuilder.Length > 0)
            {
                _inputBuilder.Remove(_inputBuilder.Length - 1, 1);
            }

            if (decimal.TryParse(_inputBuilder.ToString(), out decimal val))
            {
                _allowanceAmount = val;
                AmountTextBox.Text = "₱" + val.ToString("N2");
            }
            else
            {
                _allowanceAmount = 0;
                AmountTextBox.Text = "₱0.00";
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _inputBuilder.Clear();
            _allowanceAmount = 0;
            AmountTextBox.Text = "₱0.00";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Home home = new Home(_account, _username, _email, _balance, _isGoogleUser); // or pass user info if needed
            home.Show();
            this.Close();
        }
    }
}
