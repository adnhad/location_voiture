namespace CarRental.Data.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // "Credit Card", "Cash", "Bank Transfer"
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } // "Pending", "Completed", "Failed"
        public string TransactionId { get; set; }
        
        // Navigation properties
        public string ClientName { get; set; }
        public string VehicleInfo { get; set; }
    }
}