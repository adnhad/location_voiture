using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CarRental.Data
{
    public static class DbHelper
    {
        private static string _connectionString;

        static DbHelper()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public static void ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteScalar();
                }
            }
        }

        public static SqlDataReader ExecuteReader(string query, params SqlParameter[] parameters)
        {
            var connection = GetConnection();
            connection.Open();
            var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters);
            return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
    }
}