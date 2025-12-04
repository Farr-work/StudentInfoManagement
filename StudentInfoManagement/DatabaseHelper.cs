using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace StudentInfoManagement
{
    public class DatabaseHelper
    {
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@";

        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Đăng nhập (Giữ nguyên)
        public string AuthenticateUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_Login", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Username", username));
                        cmd.Parameters.Add(new SqlParameter("@Password", HashPassword(password)));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) return reader["RoleName"].ToString();
                        }
                    }
                }
                catch (Exception) { throw; }
            }
            return null;
        }

        // Đăng ký ADMIN (Logic mới)
        public bool RegisterAdmin(string username, string password, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Gọi thủ tục sp_RegisterAdmin thay vì sp_Register cũ
                    using (SqlCommand cmd = new SqlCommand("sp_RegisterAdmin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // Không cần truyền FullName nữa vì form đã bỏ
                        cmd.Parameters.Add(new SqlParameter("@Username", username));
                        cmd.Parameters.Add(new SqlParameter("@PasswordHash", HashPassword(password)));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int resultCode = Convert.ToInt32(reader["ResultCode"]);
                                message = reader["Message"].ToString();
                                return resultCode == 1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = "Lỗi: " + ex.Message;
                    return false;
                }
            }
            message = "Lỗi không xác định";
            return false;
        }
    }
}