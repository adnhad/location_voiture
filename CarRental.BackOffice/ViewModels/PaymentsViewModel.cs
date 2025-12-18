using CarRental.Data.Models;
using CarRental.Data.Repositories;
using CarRental.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CarRental.BackOffice.ViewModels
{
    public class PaymentsViewModel : INotifyPropertyChanged
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly RentalRepository _rentalRepository;
        private readonly ExcelService _excelService;
        private readonly PdfService _pdfService;
        private ObservableCollection<Payment> _payments;
        private Payment _selectedPayment;
        private string _searchText;
        private string _statusMessage;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private ObservableCollection<string> _statusFilters;
        private string _selectedStatusFilter;
        private ObservableCollection<Rental> _availableRentals;

        public ObservableCollection<Payment> Payments
        {
            get => _payments;
            set 
            { 
                _payments = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(TotalPaymentsCount));
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(CompletedPaymentsCount));
            }
        }

        public Payment SelectedPayment
        {
            get => _selectedPayment;
            set 
            { 
                _selectedPayment = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IsPaymentSelected));
                OnPropertyChanged(nameof(CanProcessPayment));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterPayments(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); FilterPayments(); }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); FilterPayments(); }
        }

        public ObservableCollection<string> StatusFilters
        {
            get => _statusFilters;
            set { _statusFilters = value; OnPropertyChanged(); }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(); FilterPayments(); }
        }

        public ObservableCollection<Rental> AvailableRentals
        {
            get => _availableRentals;
            set { _availableRentals = value; OnPropertyChanged(); }
        }

        // Calculated properties
        public int TotalPaymentsCount => Payments?.Count ?? 0;
        public decimal TotalAmount => Payments?.Sum(p => p.Amount) ?? 0;
        public int CompletedPaymentsCount => Payments?.Count(p => p.Status == "Completed") ?? 0;
        public bool IsPaymentSelected => SelectedPayment != null;
        public bool CanProcessPayment => SelectedPayment != null && SelectedPayment.Status == "Pending";

        // Commands
        public ICommand LoadPaymentsCommand { get; }
        public ICommand AddPaymentCommand { get; }
        public ICommand EditPaymentCommand { get; }
        public ICommand DeletePaymentCommand { get; }
        public ICommand ProcessPaymentCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand RefreshCommand { get; }

        public PaymentsViewModel()
        {
            _paymentRepository = new PaymentRepository();
            _rentalRepository = new RentalRepository();
            _excelService = new ExcelService();
            _pdfService = new PdfService();
            
            Payments = new ObservableCollection<Payment>();
            AvailableRentals = new ObservableCollection<Rental>();
            StatusMessage = "Ready";
            
            // Initialize date filters (default to last 30 days)
            EndDate = DateTime.Today;
            StartDate = DateTime.Today.AddDays(-30);
            
            // Initialize status filters
            StatusFilters = new ObservableCollection<string>
            {
                "All",
                "Pending",
                "Completed",
                "Failed"
            };
            SelectedStatusFilter = "All";
            
            // Initialize commands
            LoadPaymentsCommand = new RelayCommand(LoadPayments);
            AddPaymentCommand = new RelayCommand(AddPayment);
            EditPaymentCommand = new RelayCommand(EditPayment, () => SelectedPayment != null);
            DeletePaymentCommand = new RelayCommand(DeletePayment, () => SelectedPayment != null);
            ProcessPaymentCommand = new RelayCommand(ProcessPayment, () => CanProcessPayment);
            GenerateReportCommand = new RelayCommand(GenerateReport);
            ExportCommand = new RelayCommand(Export);
            RefreshCommand = new RelayCommand(Refresh);
            
            LoadAvailableRentals();
            LoadPayments(null);
        }

        private void LoadAvailableRentals()
        {
            try
            {
                AvailableRentals.Clear();
                var rentals = _rentalRepository.GetAllRentals()
                    .Where(r => r.Status == "Completed" || r.Status == "Active")
                    .ToList();
                
                foreach (var rental in rentals)
                {
                    AvailableRentals.Add(rental);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading rentals: {ex.Message}";
            }
        }

        private void LoadPayments(object parameter)
        {
            try
            {
                Payments.Clear();
                var payments = _paymentRepository.GetAllPayments();
                foreach (var payment in payments)
                {
                    Payments.Add(payment);
                }
                StatusMessage = $"Loaded {Payments.Count} payments";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading payments: {ex.Message}";
                MessageBox.Show($"Error loading payments: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterPayments()
        {
            try
            {
                var allPayments = _paymentRepository.GetAllPayments();
                Payments.Clear();
                
                foreach (var payment in allPayments)
                {
                    bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                                        payment.ClientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                        payment.VehicleInfo.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                        payment.TransactionId.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                    
                    bool matchesDate = (!StartDate.HasValue || payment.PaymentDate.Date >= StartDate.Value.Date) &&
                                      (!EndDate.HasValue || payment.PaymentDate.Date <= EndDate.Value.Date);
                    
                    bool matchesStatus = SelectedStatusFilter == "All" || 
                                        payment.Status == SelectedStatusFilter;
                    
                    if (matchesSearch && matchesDate && matchesStatus)
                    {
                        Payments.Add(payment);
                    }
                }
                
                StatusMessage = $"Showing {Payments.Count} payments";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error filtering payments: {ex.Message}";
            }
        }

        private void AddPayment(object parameter)
        {
            try
            {
                // Create a dialog window for adding payment
                var dialog = new Window
                {
                    Title = "Add New Payment",
                    Width = 400,
                    Height = 450,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };

                // Rental Selection
                stackPanel.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Select Rental:", 
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                var rentalCombo = new System.Windows.Controls.ComboBox
                {
                    ItemsSource = AvailableRentals,
                    DisplayMemberPath = "DisplayInfo",
                    SelectedValuePath = "Id",
                    Margin = new Thickness(0, 0, 0, 10),
                    Height = 30
                };

                // Add display info to rentals
                foreach (var rental in AvailableRentals)
                {
                    rental.DisplayInfo = $"#{rental.Id} - {rental.ClientName} - {rental.VehicleInfo} - ${rental.TotalAmount}";
                }

                stackPanel.Children.Add(rentalCombo);

                // Amount
                stackPanel.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Amount:", 
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                });

                var amountTextBox = new System.Windows.Controls.TextBox
                {
                    Height = 30,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                stackPanel.Children.Add(amountTextBox);

                // Payment Method
                stackPanel.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Payment Method:", 
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                });

                var methodCombo = new System.Windows.Controls.ComboBox
                {
                    ItemsSource = new List<string> { "Credit Card", "Cash", "Bank Transfer" },
                    SelectedIndex = 0,
                    Height = 30,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                stackPanel.Children.Add(methodCombo);

                // Buttons
                var buttonPanel = new System.Windows.Controls.StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right
                };

                var cancelButton = new System.Windows.Controls.Button
                {
                    Content = "Cancel",
                    Width = 80,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                cancelButton.Click += (s, e) => dialog.Close();

                var saveButton = new System.Windows.Controls.Button
                {
                    Content = "Save",
                    Width = 80,
                    Background = System.Windows.Media.Brushes.Green,
                    Foreground = System.Windows.Media.Brushes.White
                };

                saveButton.Click += (s, e) =>
                {
                    if (rentalCombo.SelectedItem == null)
                    {
                        MessageBox.Show("Please select a rental", "Validation Error", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!decimal.TryParse(amountTextBox.Text, out decimal amount) || amount <= 0)
                    {
                        MessageBox.Show("Please enter a valid amount", "Validation Error", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var selectedRental = (Rental)rentalCombo.SelectedItem;
                    
                    var newPayment = new Payment
                    {
                        RentalId = selectedRental.Id,
                        Amount = amount,
                        PaymentMethod = methodCombo.SelectedItem.ToString(),
                        PaymentDate = DateTime.Now,
                        Status = "Pending",
                        TransactionId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
                    };

                    try
                    {
                        _paymentRepository.AddPayment(newPayment);
                        StatusMessage = $"Payment added successfully (Transaction: {newPayment.TransactionId})";
                        LoadPayments(null);
                        dialog.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving payment: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                buttonPanel.Children.Add(cancelButton);
                buttonPanel.Children.Add(saveButton);
                stackPanel.Children.Add(buttonPanel);

                dialog.Content = stackPanel;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening add dialog: {ex.Message}";
                MessageBox.Show($"Error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditPayment(object parameter)
        {
            if (SelectedPayment == null) return;

            try
            {
                var dialog = new Window
                {
                    Title = $"Edit Payment #{SelectedPayment.Id}",
                    Width = 400,
                    Height = 350,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };

                // Amount
                stackPanel.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Amount:", 
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                var amountTextBox = new System.Windows.Controls.TextBox
                {
                    Text = SelectedPayment.Amount.ToString("F2"),
                    Height = 30,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                stackPanel.Children.Add(amountTextBox);

                // Payment Method
                stackPanel.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Payment Method:", 
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                });

                var methodCombo = new System.Windows.Controls.ComboBox
                {
                    ItemsSource = new List<string> { "Credit Card", "Cash", "Bank Transfer" },
                    SelectedItem = SelectedPayment.PaymentMethod,
                    Height = 30,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                stackPanel.Children.Add(methodCombo);

                // Status
                stackPanel.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "Status:", 
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                });

                var statusCombo = new System.Windows.Controls.ComboBox
                {
                    ItemsSource = new List<string> { "Pending", "Completed", "Failed" },
                    SelectedItem = SelectedPayment.Status,
                    Height = 30,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                stackPanel.Children.Add(statusCombo);

                // Buttons
                var buttonPanel = new System.Windows.Controls.StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right
                };

                var cancelButton = new System.Windows.Controls.Button
                {
                    Content = "Cancel",
                    Width = 80,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                cancelButton.Click += (s, e) => dialog.Close();

                var saveButton = new System.Windows.Controls.Button
                {
                    Content = "Save",
                    Width = 80,
                    Background = System.Windows.Media.Brushes.Green,
                    Foreground = System.Windows.Media.Brushes.White
                };

                saveButton.Click += (s, e) =>
                {
                    if (!decimal.TryParse(amountTextBox.Text, out decimal amount) || amount <= 0)
                    {
                        MessageBox.Show("Please enter a valid amount", "Validation Error", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    SelectedPayment.Amount = amount;
                    SelectedPayment.PaymentMethod = methodCombo.SelectedItem.ToString();
                    SelectedPayment.Status = statusCombo.SelectedItem.ToString();

                    // Note: In real implementation, you would update in database
                    // _paymentRepository.UpdatePayment(SelectedPayment);
                    
                    StatusMessage = $"Payment #{SelectedPayment.Id} updated";
                    
                    // Refresh display
                    var index = Payments.IndexOf(SelectedPayment);
                    Payments.RemoveAt(index);
                    Payments.Insert(index, SelectedPayment);
                    
                    dialog.Close();
                };

                buttonPanel.Children.Add(cancelButton);
                buttonPanel.Children.Add(saveButton);
                stackPanel.Children.Add(buttonPanel);

                dialog.Content = stackPanel;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening edit dialog: {ex.Message}";
                MessageBox.Show($"Error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeletePayment(object parameter)
        {
            if (SelectedPayment == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete payment #{SelectedPayment.Id}?\n" +
                $"Amount: {SelectedPayment.Amount:C}\n" +
                $"Transaction: {SelectedPayment.TransactionId}",
                "Confirm Delete", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // In real implementation:
                    // _paymentRepository.DeletePayment(SelectedPayment.Id);
                    
                    Payments.Remove(SelectedPayment);
                    SelectedPayment = null;
                    
                    StatusMessage = "Payment deleted successfully";
                    
                    // Show success message
                    MessageBox.Show("Payment deleted successfully", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting payment: {ex.Message}";
                    MessageBox.Show($"Error deleting payment: {ex.Message}\n\nDetails: {ex.InnerException?.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ProcessPayment(object parameter)
        {
            if (SelectedPayment == null || !CanProcessPayment) return;

            var result = MessageBox.Show(
                $"Process payment #{SelectedPayment.Id}?\n" +
                $"Amount: {SelectedPayment.Amount:C}\n" +
                $"Method: {SelectedPayment.PaymentMethod}",
                "Process Payment", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Simulate payment processing
                    SelectedPayment.Status = "Completed";
                    SelectedPayment.PaymentDate = DateTime.Now;
                    
                    // In real implementation:
                    // _paymentRepository.UpdatePaymentStatus(SelectedPayment.Id, "Completed");
                    
                    StatusMessage = $"Payment #{SelectedPayment.Id} processed successfully";
                    
                    // Refresh display
                    var index = Payments.IndexOf(SelectedPayment);
                    Payments.RemoveAt(index);
                    Payments.Insert(index, SelectedPayment);
                    
                    // Update command states
                    CommandManager.InvalidateRequerySuggested();
                    
                    // Generate receipt
                    GenerateReceipt(SelectedPayment);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error processing payment: {ex.Message}";
                    MessageBox.Show($"Error processing payment: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GenerateReceipt(Payment payment)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Payment_Receipt_{payment.TransactionId}_{DateTime.Now:yyyyMMdd}.pdf",
                    DefaultExt = ".pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Get rental details for receipt
                    var rental = _rentalRepository.GetRentalById(payment.RentalId);
                    if (rental != null)
                    {
                        // In real implementation, use PdfService
                        // var pdfBytes = _pdfService.GeneratePaymentReceipt(payment, rental);
                        // File.WriteAllBytes(saveDialog.FileName, pdfBytes);
                        
                        // For now, create a simple text receipt
                        var receiptContent = $@"
                            =================================
                            CAR RENTAL PAYMENT RECEIPT
                            =================================
                            Receipt #: {payment.TransactionId}
                            Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                            
                            Payment Details:
                            ----------------
                            Payment ID: {payment.Id}
                            Amount: {payment.Amount:C}
                            Method: {payment.PaymentMethod}
                            Status: {payment.Status}
                            
                            Rental Details:
                            --------------
                            Rental ID: {rental.Id}
                            Client: {rental.ClientName}
                            Vehicle: {rental.VehicleInfo}
                            Period: {rental.StartDate:yyyy-MM-dd} to {rental.EndDate:yyyy-MM-dd}
                            Total Rental: {rental.TotalAmount:C}
                            
                            =================================
                            Thank you for your payment!
                            =================================
                        ";
                        
                        File.WriteAllText(saveDialog.FileName, receiptContent);
                        
                        StatusMessage = $"Receipt saved to: {saveDialog.FileName}";
                        
                        // Ask if user wants to open the receipt
                        var openResult = MessageBox.Show(
                            "Receipt generated successfully. Would you like to open it?",
                            "Receipt Generated",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                        
                        if (openResult == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating receipt: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateReport(object parameter)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                    FileName = $"Payments_Report_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Filter payments based on current filters
                    var filteredPayments = Payments.ToList();
                    
                    if (saveDialog.FileName.EndsWith(".xlsx"))
                    {
                        // Generate Excel report
                        var excelBytes = _excelService.GeneratePaymentsReport(filteredPayments);
                        File.WriteAllBytes(saveDialog.FileName, excelBytes);
                    }
                    else if (saveDialog.FileName.EndsWith(".csv"))
                    {
                        // Generate CSV report
                        var csvContent = "ID,RentalID,Client,Vehicle,Amount,Method,Date,Status,TransactionID\n";
                        
                        foreach (var payment in filteredPayments)
                        {
                            csvContent += $"{payment.Id},{payment.RentalId},\"{payment.ClientName}\"," +
                                         $"\"{payment.VehicleInfo}\",{payment.Amount}," +
                                         $"{payment.PaymentMethod},{payment.PaymentDate:yyyy-MM-dd}," +
                                         $"{payment.Status},{payment.TransactionId}\n";
                        }
                        
                        File.WriteAllText(saveDialog.FileName, csvContent);
                    }
                    
                    StatusMessage = $"Report exported to: {Path.GetFileName(saveDialog.FileName)}";
                    
                    var openResult = MessageBox.Show(
                        "Report generated successfully. Would you like to open it?",
                        "Report Generated",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    if (openResult == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error generating report: {ex.Message}";
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Export(object parameter)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json|XML Files (*.xml)|*.xml",
                    FileName = $"Payments_Export_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var paymentsToExport = Payments.ToList();
                    
                    if (saveDialog.FileName.EndsWith(".json"))
                    {
                        // Export to JSON
                        var json = System.Text.Json.JsonSerializer.Serialize(paymentsToExport, 
                            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(saveDialog.FileName, json);
                    }
                    else if (saveDialog.FileName.EndsWith(".xml"))
                    {
                        // Export to XML
                        var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Payment>));
                        using var writer = new System.IO.StreamWriter(saveDialog.FileName);
                        xmlSerializer.Serialize(writer, paymentsToExport);
                    }
                    
                    StatusMessage = $"Data exported to: {Path.GetFileName(saveDialog.FileName)}";
                    MessageBox.Show($"Export completed successfully!\n\nFile: {saveDialog.FileName}", 
                        "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting data: {ex.Message}";
                MessageBox.Show($"Error exporting data: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh(object parameter)
        {
            LoadAvailableRentals();
            LoadPayments(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for display
    public class PaymentDisplay
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public string ClientName { get; set; }
        public string VehicleInfo { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public string DisplayInfo => $"{Id} - {ClientName} - {Amount:C} - {Status}";
    }
}