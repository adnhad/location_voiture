using CarRental.Data.Repositories;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CarRental.BackOffice.Views;
using CarRental.Data.Models;

namespace CarRental.BackOffice.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly UserRepository _userRepository;
        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; }

        public LoginViewModel()
        {
            _userRepository = new UserRepository();
            LoginCommand = new RelayCommand(Login);
            ExitCommand = new RelayCommand(Exit);
        }

        private void Login(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter username and password";
                return;
            }

            var user = _userRepository.Authenticate(Username, Password);
            if (user != null)
            {
                App.CurrentUser = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    Email = user.Email
                };

                ErrorMessage = string.Empty;
                
                // Open dashboard
                var dashboard = new DashboardWindow();
                dashboard.Show();
                
                // Close login window
                if (parameter is Window window)
                    window.Close();
            }
            else
            {
                ErrorMessage = "Invalid username or password";
            }
        }

        private void Exit(object parameter)
        {
            Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}