using CarRental.Data.Models;
using CarRental.Data.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CarRental.BackOffice.ViewModels
{
    public class UsersViewModel : INotifyPropertyChanged
    {
        private readonly UserRepository _userRepository;
        private ObservableCollection<User> _users;
        private User _selectedUser;
        private string _searchText;

        public ObservableCollection<User> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set { _selectedUser = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsUserSelected)); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterUsers(); }
        }

        public bool IsUserSelected => SelectedUser != null;

        public ICommand LoadUsersCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand RefreshCommand { get; }

        public UsersViewModel()
        {
            _userRepository = new UserRepository();
            Users = new ObservableCollection<User>();
            
            LoadUsersCommand = new RelayCommand(LoadUsers);
            AddUserCommand = new RelayCommand(AddUser);
            EditUserCommand = new RelayCommand(EditUser, CanEditDelete);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanEditDelete);
            RefreshCommand = new RelayCommand(Refresh);
            
            LoadUsers(null);
        }

        private void LoadUsers(object parameter)
        {
            Users.Clear();
            List<Data.Models.User> users = _userRepository.GetAllUsers();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadUsers(null);
                return;
            }

            var filtered = Users.Where(u =>
                u.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                u.Role.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
            
            Users.Clear();
            foreach (var user in filtered)
            {
                Users.Add(user);
            }
        }

        private void AddUser(object parameter)
        {
            // Implementation for adding user
            MessageBox.Show("Add User functionality would open a dialog here");
        }

        private void EditUser(object parameter)
        {
            if (SelectedUser != null)
            {
                // Implementation for editing user
                MessageBox.Show($"Edit User: {SelectedUser.Username}");
            }
        }

        private void DeleteUser(object parameter)
        {
            if (SelectedUser != null && MessageBox.Show($"Delete user {SelectedUser.Username}?", "Confirm Delete", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // Implementation for deleting user
                Users.Remove(SelectedUser);
                SelectedUser = null;
            }
        }

        private void Refresh(object parameter)
        {
            LoadUsers(null);
        }

        private bool CanEditDelete(object parameter) => SelectedUser != null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}