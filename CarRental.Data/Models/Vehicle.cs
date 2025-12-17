namespace CarRental.Data.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public string Color { get; set; }
        public decimal DailyRate { get; set; }
        public bool IsAvailable { get; set; }
        public string VehicleType { get; set; } // "Economy", "Standard", "Luxury", "SUV"
        public DateTime CreatedAt { get; set; }
    }
}