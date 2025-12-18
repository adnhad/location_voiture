using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System.Drawing;
using CarRental.Data.Models;


namespace CarRental.Services
{
    public class ExcelService : IDisposable
    {
        private bool _disposed = false;
        
        public ExcelService()
        {
           
        }

        #region Vehicle Reports

        public byte[] GenerateVehiclesReport(List<Vehicle> vehicles)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Vehicles");
                
                // Set column headers
                string[] headers = { "ID", "Make", "Model", "Year", "License Plate", "Color", 
                                    "Daily Rate", "Available", "Type", "Created Date" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }
                
                // Apply header style
                using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                    range.Style.Font.Color.SetColor(Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                
                // Add data
                for (int i = 0; i < vehicles.Count; i++)
                {
                    var vehicle = vehicles[i];
                    int row = i + 2;
                    
                    worksheet.Cells[row, 1].Value = vehicle.Id;
                    worksheet.Cells[row, 2].Value = vehicle.Make;
                    worksheet.Cells[row, 3].Value = vehicle.Model;
                    worksheet.Cells[row, 4].Value = vehicle.Year;
                    worksheet.Cells[row, 5].Value = vehicle.LicensePlate;
                    worksheet.Cells[row, 6].Value = vehicle.Color;
                    worksheet.Cells[row, 7].Value = vehicle.DailyRate;
                    worksheet.Cells[row, 8].Value = vehicle.IsAvailable ? "Yes" : "No";
                    worksheet.Cells[row, 9].Value = vehicle.VehicleType;
                    worksheet.Cells[row, 10].Value = vehicle.CreatedAt.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 10].Style.Numberformat.Format = "yyyy-mm-dd";
                    
                    // Color code availability
                    if (!vehicle.IsAvailable)
                    {
                        worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.Red);
                    }
                    else
                    {
                        worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.Green);
                    }
                    
                    // Format currency
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";
                }
                
                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                
                // Add summary statistics
                int lastRow = vehicles.Count + 3;
                worksheet.Cells[lastRow, 1].Value = "Summary Statistics";
                worksheet.Cells[lastRow, 1].Style.Font.Bold = true;
                worksheet.Cells[lastRow, 1].Style.Font.Size = 12;
                
                worksheet.Cells[lastRow + 1, 1].Value = "Total Vehicles:";
                worksheet.Cells[lastRow + 1, 2].Value = vehicles.Count;
                
                worksheet.Cells[lastRow + 2, 1].Value = "Available Vehicles:";
                worksheet.Cells[lastRow + 2, 2].Value = vehicles.Count(v => v.IsAvailable);
                
                worksheet.Cells[lastRow + 3, 1].Value = "Average Daily Rate:";
                worksheet.Cells[lastRow + 3, 2].Formula = $"AVERAGE(G2:G{vehicles.Count + 1})";
                worksheet.Cells[lastRow + 3, 2].Style.Numberformat.Format = "$#,##0.00";
                
                // Add timestamp
                worksheet.Cells[lastRow + 5, 1].Value = $"Report generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                worksheet.Cells[lastRow + 5, 1].Style.Font.Italic = true;
                
                return package.GetAsByteArray();
            }
        }

        #endregion

        #region Rental Reports

        public byte[] GenerateRentalsReport(List<Rental> rentals)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Rentals");
                
                // Set column headers
                string[] headers = { "Rental ID", "Client", "Vehicle", "Start Date", "End Date", 
                                    "Actual Return", "Total Amount", "Deposit", "Status", "Created Date" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }
                
                // Apply header style
                using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(155, 194, 230));
                    range.Style.Font.Color.SetColor(Color.Black);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                
                // Add data
                for (int i = 0; i < rentals.Count; i++)
                {
                    var rental = rentals[i];
                    int row = i + 2;
                    
                    worksheet.Cells[row, 1].Value = rental.Id;
                    worksheet.Cells[row, 2].Value = rental.ClientName;
                    worksheet.Cells[row, 3].Value = rental.VehicleInfo;
                    worksheet.Cells[row, 4].Value = rental.StartDate;
                    worksheet.Cells[row, 5].Value = rental.EndDate;
                    worksheet.Cells[row, 6].Value = rental.ActualReturnDate?.ToString("yyyy-MM-dd") ?? "N/A";
                    worksheet.Cells[row, 7].Value = rental.TotalAmount;
                    worksheet.Cells[row, 8].Value = rental.Deposit;
                    worksheet.Cells[row, 9].Value = rental.Status;
                    worksheet.Cells[row, 10].Value = rental.CreatedAt;
                    
                    // Format dates
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "yyyy-mm-dd";
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "yyyy-mm-dd";
                    worksheet.Cells[row, 10].Style.Numberformat.Format = "yyyy-mm-dd";
                    
                    // Format currency
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "$#,##0.00";
                    
                    // Color code status
                    var statusCell = worksheet.Cells[row, 9];
                    switch (rental.Status.ToLower())
                    {
                        case "active":
                            statusCell.Style.Font.Color.SetColor(Color.Green);
                            break;
                        case "reserved":
                            statusCell.Style.Font.Color.SetColor(Color.Blue);
                            break;
                        case "completed":
                            statusCell.Style.Font.Color.SetColor(Color.DarkGreen);
                            break;
                        case "cancelled":
                            statusCell.Style.Font.Color.SetColor(Color.Red);
                            break;
                    }
                }
                
                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                
                // Add summary
                int lastRow = rentals.Count + 3;
                worksheet.Cells[lastRow, 1].Value = "Summary";
                worksheet.Cells[lastRow, 1].Style.Font.Bold = true;
                worksheet.Cells[lastRow, 1].Style.Font.Size = 12;
                
                worksheet.Cells[lastRow + 1, 1].Value = "Total Rentals:";
                worksheet.Cells[lastRow + 1, 2].Value = rentals.Count;
                
                worksheet.Cells[lastRow + 2, 1].Value = "Active Rentals:";
                worksheet.Cells[lastRow + 2, 2].Value = rentals.Count(r => r.Status == "Active");
                
                worksheet.Cells[lastRow + 3, 1].Value = "Total Revenue:";
                worksheet.Cells[lastRow + 3, 2].Formula = $"SUM(G2:G{rentals.Count + 1})";
                worksheet.Cells[lastRow + 3, 2].Style.Numberformat.Format = "$#,##0.00";
                
                // Add timestamp
                worksheet.Cells[lastRow + 5, 1].Value = $"Report generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                worksheet.Cells[lastRow + 5, 1].Style.Font.Italic = true;
                
                return package.GetAsByteArray();
            }
        }

        #endregion

        #region Payment Reports

        public byte[] GeneratePaymentsReport(List<Payment> payments)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Payments");
                
                // Set column headers
                string[] headers = { "Payment ID", "Rental ID", "Client", "Vehicle", "Amount", 
                                    "Payment Method", "Payment Date", "Status", "Transaction ID" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }
                
                // Apply header style
                using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(198, 224, 180));
                    range.Style.Font.Color.SetColor(Color.Black);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                
                // Add data
                for (int i = 0; i < payments.Count; i++)
                {
                    var payment = payments[i];
                    int row = i + 2;
                    
                    worksheet.Cells[row, 1].Value = payment.Id;
                    worksheet.Cells[row, 2].Value = payment.RentalId;
                    worksheet.Cells[row, 3].Value = payment.ClientName;
                    worksheet.Cells[row, 4].Value = payment.VehicleInfo;
                    worksheet.Cells[row, 5].Value = payment.Amount;
                    worksheet.Cells[row, 6].Value = payment.PaymentMethod;
                    worksheet.Cells[row, 7].Value = payment.PaymentDate;
                    worksheet.Cells[row, 8].Value = payment.Status;
                    worksheet.Cells[row, 9].Value = payment.TransactionId;
                    
                    // Format dates
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "yyyy-mm-dd";
                    
                    // Format currency
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                    
                    // Color code status
                    var statusCell = worksheet.Cells[row, 8];
                    switch (payment.Status.ToLower())
                    {
                        case "completed":
                            statusCell.Style.Font.Color.SetColor(Color.Green);
                            break;
                        case "pending":
                            statusCell.Style.Font.Color.SetColor(Color.Orange);
                            break;
                        case "failed":
                            statusCell.Style.Font.Color.SetColor(Color.Red);
                            break;
                    }
                }
                
                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                
                // Add table for better filtering
                var tableRange = worksheet.Cells[1, 1, payments.Count + 1, headers.Length];
                var table = worksheet.Tables.Add(tableRange, "PaymentsTable");
                table.TableStyle = TableStyles.Medium6;
                
                // Add summary
                int lastRow = payments.Count + 4;
                worksheet.Cells[lastRow, 1].Value = "Summary";
                worksheet.Cells[lastRow, 1].Style.Font.Bold = true;
                worksheet.Cells[lastRow, 1].Style.Font.Size = 12;
                
                worksheet.Cells[lastRow + 1, 1].Value = "Total Payments:";
                worksheet.Cells[lastRow + 1, 2].Value = payments.Count;
                
                worksheet.Cells[lastRow + 2, 1].Value = "Total Amount:";
                worksheet.Cells[lastRow + 2, 2].Formula = $"SUM(E2:E{payments.Count + 1})";
                worksheet.Cells[lastRow + 2, 2].Style.Numberformat.Format = "$#,##0.00";
                
                worksheet.Cells[lastRow + 3, 1].Value = "Completed Payments:";
                worksheet.Cells[lastRow + 3, 2].Value = payments.Count(p => p.Status == "Completed");
                
                worksheet.Cells[lastRow + 4, 1].Value = "Pending Payments:";
                worksheet.Cells[lastRow + 4, 2].Value = payments.Count(p => p.Status == "Pending");
                
                // Add timestamp
                worksheet.Cells[lastRow + 6, 1].Value = $"Report generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                worksheet.Cells[lastRow + 6, 1].Style.Font.Italic = true;
                
                return package.GetAsByteArray();
            }
        }

        #endregion

        #region Client Reports

        public byte[] GenerateClientsReport(List<Client> clients)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Clients");
                
                // Set column headers
                string[] headers = { "Client ID", "First Name", "Last Name", "Email", "Phone", 
                                    "Address", "License Number", "License Expiry", "Created Date" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }
                
                // Apply header style
                using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 199, 206));
                    range.Style.Font.Color.SetColor(Color.Black);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                
                // Add data
                for (int i = 0; i < clients.Count; i++)
                {
                    var client = clients[i];
                    int row = i + 2;
                    
                    worksheet.Cells[row, 1].Value = client.Id;
                    worksheet.Cells[row, 2].Value = client.FirstName;
                    worksheet.Cells[row, 3].Value = client.LastName;
                    worksheet.Cells[row, 4].Value = client.Email;
                    worksheet.Cells[row, 5].Value = client.Phone;
                    worksheet.Cells[row, 6].Value = client.Address;
                    worksheet.Cells[row, 7].Value = client.LicenseNumber;
                    worksheet.Cells[row, 8].Value = client.LicenseExpiry;
                    worksheet.Cells[row, 9].Value = client.CreatedAt;
                    
                    // Format dates
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "yyyy-mm-dd";
                    worksheet.Cells[row, 9].Style.Numberformat.Format = "yyyy-mm-dd";
                    
                    // Highlight expired licenses
                    if (client.LicenseExpiry < DateTime.Now)
                    {
                        worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, 8].Style.Font.Bold = true;
                    }
                    else if (client.LicenseExpiry < DateTime.Now.AddMonths(3))
                    {
                        worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.Orange);
                    }
                }
                
                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                
                // Add summary
                int lastRow = clients.Count + 3;
                worksheet.Cells[lastRow, 1].Value = "Summary";
                worksheet.Cells[lastRow, 1].Style.Font.Bold = true;
                worksheet.Cells[lastRow, 1].Style.Font.Size = 12;
                
                worksheet.Cells[lastRow + 1, 1].Value = "Total Clients:";
                worksheet.Cells[lastRow + 1, 2].Value = clients.Count;
                
                worksheet.Cells[lastRow + 2, 1].Value = "Expired Licenses:";
                worksheet.Cells[lastRow + 2, 2].Value = clients.Count(c => c.LicenseExpiry < DateTime.Now);
                
                worksheet.Cells[lastRow + 3, 1].Value = "Licenses Expiring Soon (<3 months):";
                worksheet.Cells[lastRow + 3, 2].Value = clients.Count(c => 
                    c.LicenseExpiry >= DateTime.Now && c.LicenseExpiry < DateTime.Now.AddMonths(3));
                
                // Add timestamp
                worksheet.Cells[lastRow + 5, 1].Value = $"Report generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                worksheet.Cells[lastRow + 5, 1].Style.Font.Italic = true;
                
                return package.GetAsByteArray();
            }
        }

        #endregion

        #region Import Methods

        public List<Vehicle> ImportVehiclesFromExcel(string filePath)
        {
            var vehicles = new List<Vehicle>();
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
            
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;
                
                if (rowCount < 2) // Need at least header + 1 data row
                    return vehicles;
                
                for (int row = 2; row <= rowCount; row++) // Start from row 2 (skip header)
                {
                    try
                    {
                        var vehicle = new Vehicle
                        {
                            Make = GetCellValue(worksheet, row, 1),
                            Model = GetCellValue(worksheet, row, 2),
                            Year = ParseInt(GetCellValue(worksheet, row, 3), 2023),
                            LicensePlate = GetCellValue(worksheet, row, 4),
                            Color = GetCellValue(worksheet, row, 5),
                            DailyRate = ParseDecimal(GetCellValue(worksheet, row, 6), 50.00m),
                            IsAvailable = GetCellValue(worksheet, row, 7).ToLower() == "yes",
                            VehicleType = GetCellValue(worksheet, row, 8),
                            CreatedAt = DateTime.Now
                        };
                        
                        vehicles.Add(vehicle);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other rows
                        Console.WriteLine($"Error importing row {row}: {ex.Message}");
                    }
                }
            }
            
            return vehicles;
        }

        public List<Client> ImportClientsFromExcel(string filePath)
        {
            var clients = new List<Client>();
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
            
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;
                
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var client = new Client
                        {
                            FirstName = GetCellValue(worksheet, row, 1),
                            LastName = GetCellValue(worksheet, row, 2),
                            Email = GetCellValue(worksheet, row, 3),
                            Phone = GetCellValue(worksheet, row, 4),
                            Address = GetCellValue(worksheet, row, 5),
                            LicenseNumber = GetCellValue(worksheet, row, 6),
                            LicenseExpiry = ParseDate(GetCellValue(worksheet, row, 7), DateTime.Now.AddYears(1)),
                            CreatedAt = DateTime.Now
                        };
                        
                        clients.Add(client);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing client row {row}: {ex.Message}");
                    }
                }
            }
            
            return clients;
        }

        #endregion

        #region Helper Methods

        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cell = worksheet.Cells[row, col];
            return cell.Value?.ToString()?.Trim() ?? string.Empty;
        }

        private int ParseInt(string value, int defaultValue)
        {
            if (int.TryParse(value, out int result))
                return result;
            return defaultValue;
        }

        private decimal ParseDecimal(string value, decimal defaultValue)
        {
            if (decimal.TryParse(value, out decimal result))
                return result;
            return defaultValue;
        }

        private DateTime ParseDate(string value, DateTime defaultValue)
        {
            if (DateTime.TryParse(value, out DateTime result))
                return result;
            return defaultValue;
        }

        #endregion

        #region Advanced Features

        public byte[] GenerateDashboardReport(DashboardStatistics stats)
        {
            using (var package = new ExcelPackage())
            {
                // Summary sheet
                var summarySheet = package.Workbook.Worksheets.Add("Dashboard Summary");
                
                // Add title
                summarySheet.Cells[1, 1].Value = "Car Rental Management Dashboard";
                summarySheet.Cells[1, 1].Style.Font.Bold = true;
                summarySheet.Cells[1, 1].Style.Font.Size = 16;
                summarySheet.Cells[1, 1, 1, 3].Merge = true;
                
                summarySheet.Cells[2, 1].Value = $"Report Period: {DateTime.Now:yyyy-MM-dd}";
                summarySheet.Cells[2, 1].Style.Font.Italic = true;
                
                // Add statistics
                int row = 4;
                AddStatistic(summarySheet, ref row, "Total Vehicles", stats.TotalVehicles);
                AddStatistic(summarySheet, ref row, "Available Vehicles", stats.AvailableVehicles);
                AddStatistic(summarySheet, ref row, "Total Clients", stats.TotalClients);
                AddStatistic(summarySheet, ref row, "Active Rentals", stats.ActiveRentals);
                AddStatistic(summarySheet, ref row, "Completed Rentals", stats.CompletedRentals);
                AddStatistic(summarySheet, ref row, "Total Revenue", stats.TotalRevenue, "$#,##0.00");
                AddStatistic(summarySheet, ref row, "Monthly Revenue", stats.MonthlyRevenue, "$#,##0.00");
                
                // Auto-fit
                summarySheet.Cells[summarySheet.Dimension.Address].AutoFitColumns();
                
                return package.GetAsByteArray();
            }
        }

        private void AddStatistic(ExcelWorksheet sheet, ref int row, string label, object value, string numberFormat = null)
        {
            sheet.Cells[row, 1].Value = label + ":";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            
            sheet.Cells[row, 2].Value = value;
            
            if (!string.IsNullOrEmpty(numberFormat))
            {
                sheet.Cells[row, 2].Style.Numberformat.Format = numberFormat;
            }
            
            row++;
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        ~ExcelService()
        {
            Dispose(false);
        }

        #endregion
    }

    #region Supporting Classes

    public class DashboardStatistics
    {
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int TotalClients { get; set; }
        public int ActiveRentals { get; set; }
        public int CompletedRentals { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }

    #endregion
}