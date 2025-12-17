using CarRental.Data.Models;
using Microsoft.Data.SqlClient;

namespace CarRental.Data.Repositories
{
    public class PaymentRepository
    {
        public List<Payment> GetAllPayments()
        {
            var payments = new List<Payment>();
            var query = @"
                SELECT p.*, c.FirstName + ' ' + c.LastName as ClientName, 
                       v.Make + ' ' + v.Model as VehicleInfo
                FROM Payments p
                JOIN Rentals r ON p.RentalId = r.Id
                JOIN Clients c ON r.ClientId = c.Id
                JOIN Vehicles v ON r.VehicleId = v.Id
                ORDER BY p.PaymentDate DESC";

            using (var reader = DbHelper.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    payments.Add(new Payment
                    {
                        Id = (int)reader["Id"],
                        RentalId = (int)reader["RentalId"],
                        Amount = (decimal)reader["Amount"],
                        PaymentMethod = reader["PaymentMethod"].ToString(),
                        PaymentDate = (DateTime)reader["PaymentDate"],
                        Status = reader["Status"].ToString(),
                        TransactionId = reader["TransactionId"].ToString(),
                        ClientName = reader["ClientName"].ToString(),
                        VehicleInfo = reader["VehicleInfo"].ToString()
                    });
                }
            }
            return payments;
        }

        public void AddPayment(Payment payment)
        {
            var query = @"
                INSERT INTO Payments (RentalId, Amount, PaymentMethod, PaymentDate, Status, TransactionId)
                VALUES (@RentalId, @Amount, @PaymentMethod, @PaymentDate, @Status, @TransactionId)";

            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@RentalId", payment.RentalId),
                new SqlParameter("@Amount", payment.Amount),
                new SqlParameter("@PaymentMethod", payment.PaymentMethod),
                new SqlParameter("@PaymentDate", DateTime.Now),
                new SqlParameter("@Status", payment.Status),
                new SqlParameter("@TransactionId", payment.TransactionId ?? Guid.NewGuid().ToString()));
        }

        public void UpdatePaymentStatus(int paymentId, string status)
        {
            var query = "UPDATE Payments SET Status = @Status WHERE Id = @Id";
            
            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Id", paymentId),
                new SqlParameter("@Status", status));
        }
    }
}