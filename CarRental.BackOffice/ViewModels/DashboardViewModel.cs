using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CarRental.BackOffice.Views;

namespace CarRental.BackOffice.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private string _welcomeMessage;

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set { _welcomeMessage = value; OnPropertyChanged(); }
        }

        public ICommand ManageUsersCommand { get; }
        public ICommand ManageVehiclesCommand { get; }
        public ICommand ManageRentalsCommand { get; }
        public ICommand ManagePaymentsCommand { get; }
        public ICommand LogoutCommand { get; }

        public DashboardViewModel()
        {
            WelcomeMessage = $"Welcome, {App.CurrentUser.Username} ({App.CurrentUser.Role})";
            
            ManageUsersCommand = new RelayCommand(OpenUsers);
            ManageVehiclesCommand = new RelayCommand(OpenVehicles);
            ManageRentalsCommand = new RelayCommand(OpenRentals);
            ManagePaymentsCommand = new RelayCommand(OpenPayments);
            LogoutCommand = new RelayCommand(Logout);
        }

        private void OpenUsers(object parameter)
        {
            var usersWindow = new UsersWindow();
            usersWindow.Show();
        }

        private void OpenVehicles(object parameter)
        {
            var vehiclesWindow = new VehiclesWindow();
            vehiclesWindow.Show();
        }

        private void OpenRentals(object parameter)
        {
            var rentalsWindow = new RentalsWindow();
            rentalsWindow.Show();
        }

        private void OpenPayments(object parameter)
        {
            var paymentsWindow = new PaymentsWindow();
            paymentsWindow.Show();
        }

        private void Logout(object parameter)
        {
            App.CurrentUser = null;
            
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            
            if (parameter is Window window)
                window.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}