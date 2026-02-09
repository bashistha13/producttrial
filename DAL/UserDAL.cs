using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography; 
using System.Text;
using trial.Models;

namespace trial.DAL
{
    public class UserDAL
    {
        private readonly string _connectionString;

        public UserDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("DefaultConnection is missing.");
        }

        // VERIFY LOGIN
        public AppUser? ValidateUser(string username, string password)
        {
            AppUser? user = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LoginUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new AppUser
                            {
                                UserId = (int)reader["UserId"],
                                Username = reader["Username"]?.ToString() ?? "",
                                PasswordHash = (byte[])reader["PasswordHash"], 
                                PasswordSalt = (byte[])reader["PasswordSalt"],
                                Role = reader["Role"]?.ToString() ?? "User"
                            };
                        }
                    }
                }
            }

            if (user == null) return null; 

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null; 
            }

            return user; 
        }

        // REGISTER NEW USER (Used by Seeder)
        public void RegisterUser(string username, string password, string role)
        {
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_RegisterUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@PasswordSalt", passwordSalt);
                    cmd.Parameters.AddWithValue("@Role", role); 

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // HELPERS
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key; 
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt)) 
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }
    }
}