using CarRental.Data.Models;
using CarRental.Data.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CarRental.BackOffice.ViewModels
{
    public class RentalsViewModel : INotifyPropertyChanged
    {
        private readonly RentalRepository _rentalRepository;
        private ObservableCollection<Rental> _rentals;
        private Rental _selectedRental;
        private string _searchText;
        private string _statusMessage;
        private ObservableCollection<string> _statusFilters;
        private string _selectedStatusFilter;
        
        
        public ObservableCollection<Rental> Rentals
        {
            get => _rentals;
            set { _rentals = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActiveRentalsCount)); OnPropertyChanged(nameof(TotalRevenue)); }
        }

        public Rental SelectedRental
        {
            get => _selectedRental;
            set { _selectedRental = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsRentalSelected)); OnPropertyChanged(nameof(CanCompleteRental)); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterRentals(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> StatusFilters
        {
            get => _statusFilters;
            set { _statusFilters = value; OnPropertyChanged(); }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(); FilterRentals(); }
        }

        public int ActiveRentalsCount => Rentals?.Count(r => r.Status == "Active" || r.Status == "Reserved") ?? 0;
        public decimal TotalRevenue => Rentals?.Sum(r => r.TotalAmount) ?? 0;
        public bool IsRentalSelected => SelectedRental != null;
        public bool CanCompleteRental => SelectedRental != null && 
                                       (SelectedRental.Status == "Active" || SelectedRental.Status == "Reserved");

        public ICommand LoadRentalsCommand { get; }
        public ICommand AddRentalCommand { get; }
        public ICommand EditRentalCommand { get; }
        public ICommand DeleteRentalCommand { get; }
        public ICommand CompleteRentalCommand { get; }
        public ICommand GenerateInvoiceCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand RefreshCommand { get; }

        public RentalsViewModel()
        {
            _rentalRepository = new RentalRepository();
            Rentals = new ObservableCollection<Rental>();
            StatusMessage = "Ready";
            
            // Initialize status filters
            StatusFilters = new ObservableCollection<string>
            {
                "All",
                "Active",
                "Reserved",
                "Completed",
                "Cancelled"
            };
            SelectedStatusFilter = "All";
            
            LoadRentalsCommand = new RelayCommand(LoadRentals);
            AddRentalCommand = new RelayCommand(AddRental);
            EditRentalCommand = new RelayCommand(EditRental, CanEditDelete);
            CompleteRentalCommand = new RelayCommand(
                CompleteRental,
                _ => CanCompleteRental
            );

            GenerateInvoiceCommand = new RelayCommand(
                GenerateInvoice,
                _ => IsRentalSelected
            );
            GenerateReportCommand = new RelayCommand(GenerateReport);
            RefreshCommand = new RelayCommand(Refresh);
            
            LoadRentals(null);
        }
        
        private void LoadRentals(object parameter)
        {
            try
            {
                Rentals.Clear();
                var rentals = _rentalRepository.GetAllRentals();
                foreach (var rental in rentals)
                {
                    Rentals.Add(rental);
                }
                StatusMessage = $"Loaded {Rentals.Count} rentals";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading rentals: {ex.Message}";
                MessageBox.Show($"Error loading rentals: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterRentals()
        {
            try
            {
                var allRentals = _rentalRepository.GetAllRentals();
                Rentals.Clear();
                
                foreach (var rental in allRentals)
                {
                    bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                                        rental.ClientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                        rental.VehicleInfo.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                    
                    bool matchesStatus = SelectedStatusFilter == "All" || 
                                        rental.Status == SelectedStatusFilter;
                    
                    if (matchesSearch && matchesStatus)
                    {
                        Rentals.Add(rental);
                    }
                }
                
                StatusMessage = $"Showing {Rentals.Count} rentals";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error filtering rentals: {ex.Message}";
            }
        }

        private void AddRental(object parameter)
        {
            MessageBox.Show("New Rental functionality would open a dialog here", "New Rental", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            
            // In real implementation:
            // 1. Open new rental dialog with client and vehicle selection
            // 2. Calculate dates and amounts
            // 3. Save to database
            // 4. Refresh list
        }

        private void EditRental(object parameter)
        {
            if (SelectedRental != null)
            {
                MessageBox.Show($"Edit Rental #{SelectedRental.Id}", "Edit Rental", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // In real implementation:
                // 1. Open edit dialog
                // 2. Update rental details
                // 3. Save changes
                // 4. Refresh list
            }
        }

        private void CompleteRental(object parameter)
        {
            if (SelectedRental != null && CanCompleteRental)
            {
                if (MessageBox.Show($"Complete rental #{SelectedRental.Id} for {SelectedRental.ClientName}?", 
                    "Complete Rental", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _rentalRepository.CompleteRental(SelectedRental.Id, DateTime.Now);
                        SelectedRental.Status = "Completed";
                        SelectedRental.ActualReturnDate = DateTime.Now;
                        
                        StatusMessage = $"Rental #{SelectedRental.Id} marked as completed";
                        
                        // Refresh display
                        var index = Rentals.IndexOf(SelectedRental);
                        Rentals.RemoveAt(index);
                        Rentals.Insert(index, SelectedRental);
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"Error completing rental: {ex.Message}";
                        MessageBox.Show($"Error completing rental: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void GenerateInvoice(object parameter)
        {
            if (SelectedRental != null)
            {
                MessageBox.Show($"Generate invoice for rental #{SelectedRental.Id}", "Generate Invoice", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // In real implementation:
                // 1. Use PdfService to generate invoice
                // 2. Save or print the PDF
            }
        }

        private void GenerateReport(object parameter)
        {
            MessageBox.Show("Generate rentals report", "Generate Report", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            
            // In real implementation:
            // 1. Generate Excel report using ExcelService
            // 2. Show save dialog
        }

        private void Refresh(object parameter)
        {
            LoadRentals(null);
        }

        private bool CanEditDelete(object parameter) => SelectedRental != null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}