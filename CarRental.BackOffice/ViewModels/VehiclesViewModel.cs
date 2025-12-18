using CarRental.Data.Models;
using CarRental.Data.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CarRental.BackOffice.ViewModels
{
    public class VehiclesViewModel : INotifyPropertyChanged
    {
        private readonly VehicleRepository _vehicleRepository;
        private ObservableCollection<Vehicle> _vehicles;
        private Vehicle _selectedVehicle;
        private string _searchText;
        private string _statusMessage;

        public ObservableCollection<Vehicle> Vehicles
        {
            get => _vehicles;
            set { _vehicles = value; OnPropertyChanged(); OnPropertyChanged(nameof(VehicleCount)); }
        }

        public Vehicle SelectedVehicle
        {
            get => _selectedVehicle;
            set { _selectedVehicle = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVehicleSelected)); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterVehicles(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public int VehicleCount => Vehicles?.Count ?? 0;
        public bool IsVehicleSelected => SelectedVehicle != null;

        public ICommand LoadVehiclesCommand { get; }
        public ICommand AddVehicleCommand { get; }
        public ICommand EditVehicleCommand { get; }
        public ICommand DeleteVehicleCommand { get; }
        public ICommand ToggleAvailabilityCommand { get; }
        public ICommand RefreshCommand { get; }

        public VehiclesViewModel()
        {
            _vehicleRepository = new VehicleRepository();
            Vehicles = new ObservableCollection<Vehicle>();
            StatusMessage = "Ready";
            
            LoadVehiclesCommand = new RelayCommand(LoadVehicles);
            AddVehicleCommand = new RelayCommand(AddVehicle);
            EditVehicleCommand = new RelayCommand(EditVehicle, CanEditDelete);
            DeleteVehicleCommand = new RelayCommand(DeleteVehicle, CanEditDelete);
            ToggleAvailabilityCommand = new RelayCommand(ToggleAvailability, CanEditDelete);
            RefreshCommand = new RelayCommand(Refresh);
            
            LoadVehicles(null);
        }

        private void LoadVehicles(object parameter)
        {
            try
            {
                Vehicles.Clear();
                var vehicles = _vehicleRepository.GetAllVehicles();
                foreach (var vehicle in vehicles)
                {
                    Vehicles.Add(vehicle);
                }
                StatusMessage = $"Loaded {VehicleCount} vehicles";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading vehicles: {ex.Message}";
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterVehicles()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadVehicles(null);
                return;
            }

            var filtered = Vehicles.Where(v =>
                v.Make.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                v.Model.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                v.LicensePlate.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                v.VehicleType.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
            
            Vehicles.Clear();
            foreach (var vehicle in filtered)
            {
                Vehicles.Add(vehicle);
            }
            StatusMessage = $"Found {VehicleCount} vehicles matching '{SearchText}'";
        }

        private void AddVehicle(object parameter)
        {
            // Show add vehicle dialog
            MessageBox.Show("Add Vehicle functionality would open a dialog here", "Add Vehicle", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            
            // In a real implementation, you would:
            // 1. Open a dialog window for vehicle details
            // 2. Create new Vehicle object
            // 3. Call _vehicleRepository.AddVehicle(newVehicle)
            // 4. Refresh the list
        }

        private void EditVehicle(object parameter)
        {
            if (SelectedVehicle != null)
            {
                MessageBox.Show($"Edit Vehicle: {SelectedVehicle.Make} {SelectedVehicle.Model}", "Edit Vehicle", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // In real implementation:
                // 1. Open edit dialog with selected vehicle details
                // 2. Update vehicle in database
                // 3. Refresh the list
            }
        }

        private void DeleteVehicle(object parameter)
        {
            if (SelectedVehicle != null && MessageBox.Show(
                $"Delete vehicle {SelectedVehicle.Make} {SelectedVehicle.Model} ({SelectedVehicle.LicensePlate})?",
                "Confirm Delete", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // In real implementation: _vehicleRepository.DeleteVehicle(SelectedVehicle.Id);
                    Vehicles.Remove(SelectedVehicle);
                    StatusMessage = $"Vehicle deleted successfully";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting vehicle: {ex.Message}";
                    MessageBox.Show($"Error deleting vehicle: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToggleAvailability(object parameter)
        {
            if (SelectedVehicle != null)
            {
                try
                {
                    SelectedVehicle.IsAvailable = !SelectedVehicle.IsAvailable;
                    _vehicleRepository.UpdateVehicleAvailability(SelectedVehicle.Id, SelectedVehicle.IsAvailable);
                    
                    StatusMessage = $"Vehicle availability updated to: {(SelectedVehicle.IsAvailable ? "Available" : "Not Available")}";
                    
                    // Refresh the display
                    var index = Vehicles.IndexOf(SelectedVehicle);
                    Vehicles.RemoveAt(index);
                    Vehicles.Insert(index, SelectedVehicle);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error updating availability: {ex.Message}";
                    MessageBox.Show($"Error updating availability: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Refresh(object parameter)
        {
            LoadVehicles(null);
        }

        private bool CanEditDelete(object parameter) => SelectedVehicle != null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}