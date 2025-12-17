using CarRental.Data.Models;
using Microsoft.Data.SqlClient;

namespace CarRental.Data.Repositories
{
    public class RentalRepository
    {
        public List<Rental> GetAllRentals()
        {
            var rentals = new List<Rental>();
            var query = @"
                SELECT r.*, c.FirstName + ' ' + c.LastName as ClientName, 
                       v.Make + ' ' + v.Model + ' (' + v.LicensePlate + ')' as VehicleInfo
                FROM Rentals r
                JOIN Clients c ON r.ClientId = c.Id
                JOIN Vehicles v ON r.VehicleId = v.Id
                ORDER BY r.CreatedAt DESC";

            using (var reader = DbHelper.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    rentals.Add(new Rental
                    {
                        Id = (int)reader["Id"],
                        ClientId = (int)reader["ClientId"],
                        VehicleId = (int)reader["VehicleId"],
                        StartDate = (DateTime)reader["StartDate"],
                        EndDate = (DateTime)reader["EndDate"],
                        ActualReturnDate = reader["ActualReturnDate"] as DateTime?,
                        TotalAmount = (decimal)reader["TotalAmount"],
                        Deposit = (decimal)reader["Deposit"],
                        Status = reader["Status"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"],
                        ClientName = reader["ClientName"].ToString(),
                        VehicleInfo = reader["VehicleInfo"].ToString()
                    });
                }
            }
            return rentals;
        }

        public void AddRental(Rental rental)
        {
            var query = @"
                INSERT INTO Rentals (ClientId, VehicleId, StartDate, EndDate, TotalAmount, Deposit, Status, CreatedAt)
                VALUES (@ClientId, @VehicleId, @StartDate, @EndDate, @TotalAmount, @Deposit, @Status, @CreatedAt)";

            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@ClientId", rental.ClientId),
                new SqlParameter("@VehicleId", rental.VehicleId),
                new SqlParameter("@StartDate", rental.StartDate),
                new SqlParameter("@EndDate", rental.EndDate),
                new SqlParameter("@TotalAmount", rental.TotalAmount),
                new SqlParameter("@Deposit", rental.Deposit),
                new SqlParameter("@Status", rental.Status),
                new SqlParameter("@CreatedAt", DateTime.Now));
        }

        public void UpdateRentalStatus(int rentalId, string status)
        {
            var query = "UPDATE Rentals SET Status = @Status WHERE Id = @Id";
            
            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Id", rentalId),
                new SqlParameter("@Status", status));
        }

        public void CompleteRental(int rentalId, DateTime actualReturnDate)
        {
            var query = @"
                UPDATE Rentals 
                SET Status = 'Completed', ActualReturnDate = @ActualReturnDate
                WHERE Id = @Id";
            
            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Id", rentalId),
                new SqlParameter("@ActualReturnDate", actualReturnDate));
        }
    }
}