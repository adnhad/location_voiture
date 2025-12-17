using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using CarRental.Data.Models;
using System.IO;

namespace CarRental.Services
{
    public class PdfService
    {
        public byte[] GenerateRentalInvoice(Rental rental, Client client, Vehicle vehicle)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new PdfWriter(stream))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);
                        
                        // Title
                        document.Add(new Paragraph("Car Rental Invoice")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(20)
                            );
                        
                        document.Add(new Paragraph("\n"));
                        
                        document.Add(new Paragraph("Car Rental Management System")
                            .SetTextAlignment(TextAlignment.CENTER));
                        document.Add(new Paragraph("123 Rental Street, City, Country")
                            .SetTextAlignment(TextAlignment.CENTER));
                        document.Add(new Paragraph("Phone: (123) 456-7890 | Email: info@carrental.com")
                            .SetTextAlignment(TextAlignment.CENTER));
                        
                        document.Add(new Paragraph("\n"));
                        
                        // Invoice Details
                        var table = new Table(2);
                        table.AddCell("Invoice Number:");
                        table.AddCell($"INV-{rental.Id:00000}");
                        table.AddCell("Invoice Date:");
                        table.AddCell(DateTime.Now.ToString("yyyy-MM-dd"));
                        table.AddCell("Rental ID:");
                        table.AddCell(rental.Id.ToString());
                        
                        document.Add(table);
                        document.Add(new Paragraph("\n"));
                        
                        // Client Information
                        document.Add(new Paragraph("Client Information")
                            );
                        
                        var clientTable = new Table(2);
                        clientTable.AddCell("Name:");
                        clientTable.AddCell($"{client.FirstName} {client.LastName}");
                        clientTable.AddCell("Email:");
                        clientTable.AddCell(client.Email);
                        clientTable.AddCell("Phone:");
                        clientTable.AddCell(client.Phone);
                        clientTable.AddCell("License Number:");
                        clientTable.AddCell(client.LicenseNumber);
                        
                        document.Add(clientTable);
                        document.Add(new Paragraph("\n"));
                        
                        // Vehicle Information
                        document.Add(new Paragraph("Vehicle Information")
                            );
                        
                        var vehicleTable = new Table(2);
                        vehicleTable.AddCell("Vehicle:");
                        vehicleTable.AddCell($"{vehicle.Make} {vehicle.Model} ({vehicle.Year})");
                        vehicleTable.AddCell("License Plate:");
                        vehicleTable.AddCell(vehicle.LicensePlate);
                        vehicleTable.AddCell("Color:");
                        vehicleTable.AddCell(vehicle.Color);
                        vehicleTable.AddCell("Type:");
                        vehicleTable.AddCell(vehicle.VehicleType);
                        
                        document.Add(vehicleTable);
                        document.Add(new Paragraph("\n"));
                        
                        // Rental Details
                        document.Add(new Paragraph("Rental Details")
                            );
                        
                        var rentalTable = new Table(2);
                        rentalTable.AddCell("Start Date:");
                        rentalTable.AddCell(rental.StartDate.ToString("yyyy-MM-dd"));
                        rentalTable.AddCell("End Date:");
                        rentalTable.AddCell(rental.EndDate.ToString("yyyy-MM-dd"));
                        rentalTable.AddCell("Daily Rate:");
                        rentalTable.AddCell($"${vehicle.DailyRate:F2}");
                        
                        int days = (rental.EndDate - rental.StartDate).Days;
                        rentalTable.AddCell("Rental Days:");
                        rentalTable.AddCell(days.ToString());
                        rentalTable.AddCell("Total Amount:");
                        rentalTable.AddCell($"${rental.TotalAmount:F2}");
                        rentalTable.AddCell("Deposit:");
                        rentalTable.AddCell($"${rental.Deposit:F2}");
                        rentalTable.AddCell("Status:");
                        rentalTable.AddCell(rental.Status);
                        
                        document.Add(rentalTable);
                        document.Add(new Paragraph("\n"));
                        
                        // Terms and Conditions
                        document.Add(new Paragraph("Terms and Conditions"));
                        document.Add(new Paragraph("1. Vehicle must be returned in the same condition."));
                        document.Add(new Paragraph("2. Late returns will incur additional charges."));
                        document.Add(new Paragraph("3. Fuel is not included in the rental price."));
                        document.Add(new Paragraph("4. Insurance coverage as per agreement."));
                        
                        // Footer
                        document.Add(new Paragraph("\n\n"));
                        document.Add(new Paragraph("Thank you for choosing our service!")
                            .SetTextAlignment(TextAlignment.CENTER));
                        
                        document.Close();
                    }
                }
                return stream.ToArray();
            }
        }
    }
}