using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Finals_temp
{
    public partial class AddExpense : Window
    {
        DataClasses2DataContext db = new DataClasses2DataContext(Properties.Settings.Default.Expense_TrackerConnectionString);
        private StringBuilder _inputBuilder = new StringBuilder();
        public decimal ExpenseAmount { get; private set; } = 0m;

        private string _account;
        private string _username;
        private string _email;
        private decimal _balance;
        private bool _isGoogleUser;

        public AddExpense(string account, string username, string email, decimal balance, bool isGoogleUser)
        {
            InitializeComponent();
            _account = account;
            _username = username;
            _email = email;
            _balance = balance;
            _isGoogleUser = isGoogleUser;

        }

        public string AmountDisplay
        {
            get => AmountText.Text;
            set
            {
                _inputBuilder.Clear();
                AmountText.Text = value;
            }
        }

        private void CalculatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string content = btn.Content.ToString();

                switch (content)
                {
                    case "C":
                        AmountDisplay = "₱0.00";
                        return;

                    case "⌫":
                        if (_inputBuilder.Length > 0)
                            _inputBuilder.Remove(_inputBuilder.Length - 1, 1);
                        break;

                    case "=":
                        try
                        {
                            var result = new DataTable().Compute(_inputBuilder.ToString(), null);
                            _inputBuilder.Clear();
                            _inputBuilder.Append(Convert.ToDecimal(result).ToString("F2"));
                        }
                        catch
                        {
                            _inputBuilder.Clear();
                            _inputBuilder.Append("Error");
                        }
                        break;

                    default:
                        _inputBuilder.Append(content);
                        break;
                }

                if (_inputBuilder.ToString() == "Error")
                {
                    AmountText.Text = "Error";
                }
                else if (decimal.TryParse(_inputBuilder.ToString(), out decimal val))
                {
                    AmountText.Text = "₱" + val.ToString("N2"); // Thousands separator + 2 decimals
                }
                else
                {
                    AmountText.Text = "₱0.00";
                }
            }
        }

        private void SaveExpense_Click(object sender, RoutedEventArgs e)
        {
            // Remove ₱ and commas from amount for clean value
            string amountStr = AmountText.Text.Replace("₱", "").Replace(",", "");
            string notes = NotesTextBox.Text.Trim();
            if (notes == "Add notes") notes = "";

            ComboBoxItem selectedItem = CategoryComboBox.SelectedItem as ComboBoxItem;
            string categoryName = selectedItem?.Content.ToString() ?? "Other";

            string categoryID;
            if (categoryName == "💡 Utilities")
                categoryID = "C02";
            else if (categoryName == "🚗 Transportation")
                categoryID = "C03";
            else if (categoryName == "🍽 Food")
                categoryID = "C04";
            else
                categoryID = "C05";


            if (decimal.TryParse(amountStr, out decimal amountDecimal))
            {
                try
                {
                    // Generate new Expense_ID like E1, E2, E3...
                    int lastNum = 0;

                    // Get all existing Expense_IDs from DB
                    var allIds = db.ExpenseTables
                                   .Select(x => x.Expense_ID)
                                   .ToList();

                    var idNumbers = new List<int>();
                    foreach (var id in allIds)
                    {
                        if (id.StartsWith("E") && int.TryParse(id.Substring(1), out int num))
                        {
                            idNumbers.Add(num);
                        }
                    }

                    if (idNumbers.Any())
                        lastNum = idNumbers.Max();

                    string newId = "E" + (lastNum + 1);

                    // Create new expense record
                    ExpenseTable newExpense = new ExpenseTable
                    {
                        Expense_ID = newId,
                        Account = _account,
                        Category_ID = categoryID,
                        Date = DateTime.Today,
                        Amount = amountDecimal,
                        Notes = notes
                    };
                    ExpenseAmount = amountDecimal;
                    this.DialogResult = true;
                    //this.Close();

                    // Save to DB
                    db.ExpenseTables.InsertOnSubmit(newExpense);
                    db.SubmitChanges();

                    // Confirmation
                    MessageBox.Show($"Saved:\nID: {newId}\nCategory: {categoryName} ({categoryID})\nAmount: ₱{amountDecimal:N2}\nNotes: {notes}",
                                    "Expense Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save expense: " + ex.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Invalid amount format.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Reset fields
            _inputBuilder.Clear();
            AmountText.Text = "₱0.00";
            NotesTextBox.Text = "";
            CategoryComboBox.SelectedIndex = -1;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Home home = new Home(_account, _username, _email, _balance, _isGoogleUser); // or pass user info if needed
            home.Show();
            this.Close();
        }
    }
}
