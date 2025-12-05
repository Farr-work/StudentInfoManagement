using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
// Đã xóa System.Security.Cryptography vì không dùng nữa

namespace StudentInfoManagement
{
    public class DatabaseHelper
    {
        // Chuỗi kết nối của bạn (SmarterASP.NET)
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        // --- 1. XỬ LÝ ĐĂNG NHẬP & TÀI KHOẢN (AUTH) ---

        // Đăng nhập: Truyền password thô vào Stored Procedure
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

                        // QUAN TRỌNG: Truyền trực tiếp password, không Hash
                        cmd.Parameters.Add(new SqlParameter("@Password", password));

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

        // Đăng ký ADMIN: Truyền password thô
        public bool RegisterAdmin(string username, string password, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_RegisterAdmin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Username", username));

                        // QUAN TRỌNG: Truyền trực tiếp password, không Hash
                        cmd.Parameters.Add(new SqlParameter("@Password", password));

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

        // --- 2. XỬ LÝ DỮ LIỆU SINH VIÊN (DATA) ---

        

    
    }
}