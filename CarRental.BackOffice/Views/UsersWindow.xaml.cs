using System.Windows;

namespace CarRental.BackOffice.Views
{
    public partial class UsersWindow : Window
    {
        public UsersWindow()
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