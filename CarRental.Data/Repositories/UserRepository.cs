using CarRental.Data.Models;
using Microsoft.Data.SqlClient;


namespace CarRental.Data.Repositories
{
    public class UserRepository
    {
        public User Authenticate(string username, string password)
        {
            var query = @"
                SELECT Id, Username, Email, Role, CreatedAt, IsActive 
                FROM Users 
                WHERE Username = @Username AND Password = @Password AND IsActive = 1";

            using (var reader = DbHelper.ExecuteReader(query,
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password)))
            {
                if (reader.Read())
                {
                    return new User
                    {
                        Id = (int)reader["Id"],
                        Username = reader["Username"].ToString(),
                        Email = reader["Email"].ToString(),
                        Role = reader["Role"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"],
                        IsActive = (bool)reader["IsActive"]
                    };
                }
                return null;
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            var query = "SELECT * FROM Users ORDER BY CreatedAt DESC";

            using (var reader = DbHelper.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = (int)reader["Id"],
                        Username = reader["Username"].ToString(),
                        Email = reader["Email"].ToString(),
                        Role = reader["Role"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"],
                        IsActive = (bool)reader["IsActive"]
                    });
                }
            }
            return users;
        }

        public void AddUser(User user)
        {
            var query = @"
                INSERT INTO Users (Username, Password, Email, Role, CreatedAt, IsActive)
                VALUES (@Username, @Password, @Email, @Role, @CreatedAt, @IsActive)";

            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Username", user.Username),
                new SqlParameter("@Password", user.Password),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@Role", user.Role),
                new SqlParameter("@CreatedAt", DateTime.Now),
                new SqlParameter("@IsActive", true));
        }

        public void UpdateUser(User user)
        {
            var query = @"
                UPDATE Users 
                SET Username = @Username, Email = @Email, Role = @Role, IsActive = @IsActive
                WHERE Id = @Id";

            DbHelper.ExecuteNonQuery(query,
                new SqlParameter("@Id", user.Id),
                new SqlParameter("@Username", user.Username),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@Role", user.Role),
                new SqlParameter("@IsActive", user.IsActive));
        }
    }
}