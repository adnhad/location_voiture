using CarRental.Data.Models;
using Microsoft.Data.SqlClient;

namespace CarRental.Data.Repositories
{
    public class VehicleRepository
    {
        public List<Vehicle> GetAllVehicles()
        {
            var vehicles = new List<Vehicle>();
            var query = "SELECT * FROM Vehicles ORDER BY CreatedAt DESC";

            using (var reader = DbHelper.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    vehicles.Add(new Vehicle
                    {
                        Id = (int)reader["Id"],
                        Make = reader["Make"].ToString(),
                        Model = reader["Model"].ToString(),
                        Year = (int)reader["Year"],
                        LicensePlate = reader["LicensePlate"].ToString(),
                        Color = reader["Color"].ToString(),
                        DailyRate = (decimal)reader["DailyRate"],
                        IsAvailable = (bool)reader["IsAvailable"],
                        VehicleType = reader["VehicleType"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
            }
            return vehicles;
        }

        public List<Vehicle> GetAvailableVehicles()
        {
            var vehicles = new List<Vehicle>();
            var query = "SELECT * FROM Vehicles WHERE IsAvailable = 1 ORDER BY CreatedAt DESC";

            using (var reader = DbHelper.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    vehicles.Add(new Vehicle
                    {
                        Id = (int)reader["Id"],
                        Make = reader["Make"].ToString(),
                        Model = reader["Model"].ToString(),
                        Year = (int)reader["Year"],
                        LicensePlate = reader["LicensePlate"].ToString(),
                        Color = reader["Color"].ToString(),
                        DailyRate = (decimal)reader["DailyRate"],
                        IsAvailable = (bool)reader["IsAvailable"],
                        VehicleType = reader["VehicleType"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
            }
            return vehicles;
        }

        public void AddVehicle(Vehicle vehicle)
        {
            var query = @"
                INSERT INTO Vehicles (Make, Model, Year, LicensePlate, Color, DailyRate, IsAvailable, VehicleType, CreatedAt)
                VALUES (@Make, @Model, @Year, @LicensePlate, @Color, @DailyRate, @IsAvailable, @VehicleType, @CreatedAt)";

            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Make", vehicle.Make),
                new SqlParameter("@Model", vehicle.Model),
                new SqlParameter("@Year", vehicle.Year),
                new SqlParameter("@LicensePlate", vehicle.LicensePlate),
                new SqlParameter("@Color", vehicle.Color),
                new SqlParameter("@DailyRate", vehicle.DailyRate),
                new SqlParameter("@IsAvailable", vehicle.IsAvailable),
                new SqlParameter("@VehicleType", vehicle.VehicleType),
                new SqlParameter("@CreatedAt", DateTime.Now));
        }

        public void UpdateVehicle(Vehicle vehicle)
        {
            var query = @"
                UPDATE Vehicles 
                SET Make = @Make, Model = @Model, Year = @Year, LicensePlate = @LicensePlate,
                    Color = @Color, DailyRate = @DailyRate, IsAvailable = @IsAvailable, VehicleType = @VehicleType
                WHERE Id = @Id";

            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Id", vehicle.Id),
                new SqlParameter("@Make", vehicle.Make),
                new SqlParameter("@Model", vehicle.Model),
                new SqlParameter("@Year", vehicle.Year),
                new SqlParameter("@LicensePlate", vehicle.LicensePlate),
                new SqlParameter("@Color", vehicle.Color),
                new SqlParameter("@DailyRate", vehicle.DailyRate),
                new SqlParameter("@IsAvailable", vehicle.IsAvailable),
                new SqlParameter("@VehicleType", vehicle.VehicleType));
        }

        public void UpdateVehicleAvailability(int vehicleId, bool isAvailable)
        {
            var query = "UPDATE Vehicles SET IsAvailable = @IsAvailable WHERE Id = @Id";
            
            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Id", vehicleId),
                new SqlParameter("@IsAvailable", isAvailable));
        }
    }
}