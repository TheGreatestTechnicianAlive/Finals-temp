using System.Windows;

namespace Finals_temp
{
    public partial class TransactionDetail : Window
    {
        public TransactionDetail(string date, string category, decimal amount, string notes)
        {
            InitializeComponent();

            DateText.Text = $"Date: {date}";
            CategoryText.Text = $"Category: {category}";
            AmountText.Text = $"Amount: ₱{amount:N2}";
            NotesText.Text = string.IsNullOrWhiteSpace(notes) ? "Notes: None" : $"Notes: {notes}";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }
    }
}
