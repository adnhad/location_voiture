using CarRental.BackOffice.ViewModels;
using System.Windows;

namespace CarRental.BackOffice.Views
{
    public partial class VehiclesWindow : Window
    {
        public VehiclesWindow()
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