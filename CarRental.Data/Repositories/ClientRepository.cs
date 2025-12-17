using CarRental.Data.Models;
using Microsoft.Data.SqlClient;

namespace CarRental.Data.Repositories
{
    public class ClientRepository
    {
        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            var query = "SELECT * FROM Clients ORDER BY CreatedAt DESC";

            using (var reader = DbHelper.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    clients.Add(new Client
                    {
                        Id = (int)reader["Id"],
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Address = reader["Address"].ToString(),
                        LicenseNumber = reader["LicenseNumber"].ToString(),
                        LicenseExpiry = (DateTime)reader["LicenseExpiry"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
            }
            return clients;
        }

        public Client GetClientById(int id)
        {
            var query = "SELECT * FROM Clients WHERE Id = @Id";
            
            using (var reader = DbHelper.ExecuteReader(query, new SqlParameter("@Id", id)))
            {
                if (reader.Read())
                {
                    return new Client
                    {
                        Id = (int)reader["Id"],
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Address = reader["Address"].ToString(),
                        LicenseNumber = reader["LicenseNumber"].ToString(),
                        LicenseExpiry = (DateTime)reader["LicenseExpiry"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    };
                }
                return null;
            }
        }

        public void AddClient(Client client)
        {
            var query = @"
                INSERT INTO Clients (FirstName, LastName, Email, Phone, Address, LicenseNumber, LicenseExpiry, CreatedAt)
                VALUES (@FirstName, @LastName, @Email, @Phone, @Address, @LicenseNumber, @LicenseExpiry, @CreatedAt)";

            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@FirstName", client.FirstName),
                new SqlParameter("@LastName", client.LastName),
                new SqlParameter("@Email", client.Email),
                new SqlParameter("@Phone", client.Phone),
                new SqlParameter("@Address", client.Address),
                new SqlParameter("@LicenseNumber", client.LicenseNumber),
                new SqlParameter("@LicenseExpiry", client.LicenseExpiry),
                new SqlParameter("@CreatedAt", DateTime.Now));
        }

        public void UpdateClient(Client client)
        {
            var query = @"
                UPDATE Clients 
                SET FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                    Phone = @Phone, Address = @Address, LicenseNumber = @LicenseNumber, 
                    LicenseExpiry = @LicenseExpiry
                WHERE Id = @Id";

            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Id", client.Id),
                new SqlParameter("@FirstName", client.FirstName),
                new SqlParameter("@LastName", client.LastName),
                new SqlParameter("@Email", client.Email),
                new SqlParameter("@Phone", client.Phone),
                new SqlParameter("@Address", client.Address),
                new SqlParameter("@LicenseNumber", client.LicenseNumber),
                new SqlParameter("@LicenseExpiry", client.LicenseExpiry));
        }
    }
}