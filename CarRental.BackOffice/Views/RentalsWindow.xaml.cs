using CarRental.BackOffice.ViewModels;
using System.Windows;

namespace CarRental.BackOffice.Views
{
    public partial class RentalsWindow : Window
    {
        public RentalsWindow()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var dashboard = new DashboardWindow();
            dashboard.Show();
            this.Close();
        }
    }
}