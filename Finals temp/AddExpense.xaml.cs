using System;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Finals_temp
{
    public partial class AddExpense : Window
    {
        private StringBuilder _inputBuilder = new StringBuilder();

        public AddExpense()
        {
            InitializeComponent();
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
            string amount = AmountText.Text.Replace("₱", "").Replace(",", "");
            string notes = NotesTextBox.Text.Trim();

            ComboBoxItem selectedItem = CategoryComboBox.SelectedItem as ComboBoxItem;
            string category = selectedItem?.Content.ToString() ?? "Other";

            MessageBox.Show($"Saved:\nCategory: {category}\nAmount: ₱{amount}\nNotes: {notes}",
                            "Expense Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reset fields
            _inputBuilder.Clear();
            AmountText.Text = "₱0.00";
            NotesTextBox.Text = "";
            CategoryComboBox.SelectedIndex = -1;
        }
    }
}
