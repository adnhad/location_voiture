namespace CarRental.Data.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int VehicleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Deposit { get; set; }
        public string Status { get; set; } // "Reserved", "Active", "Completed", "Cancelled"
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties (for display)
        public string ClientName { get; set; }
        public string VehicleInfo { get; set; }
    }
}