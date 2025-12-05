using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace StudentInfoManagement
{
    public class DatabaseHelper
    {
        // Chuỗi kết nối chuẩn đồng bộ cho toàn project
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        // --- 1. XỬ LÝ ĐĂNG NHẬP (AUTH) ---
        public string AuthenticateUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // SỬA: Dùng câu lệnh SQL trực tiếp thay vì Stored Procedure
                    // Join bảng Users và Roles để lấy tên quyền (Admin/Student...)
                    string sql = @"
                        SELECT r.RoleName 
                        FROM Users u
                        JOIN Roles r ON u.RoleID = r.RoleID
                        WHERE u.Username = @Username AND u.Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password); // Lưu ý: Nên dùng mã hóa MD5/SHA256 trong thực tế

                        object result = cmd.ExecuteScalar();

                        // Nếu tìm thấy user khớp user/pass -> trả về RoleName (VD: 'Admin')
                        if (result != null)
                        {
                            return result.ToString();
                        }
                    }
                }
                catch (Exception) { throw; }
            }
            return null; // Đăng nhập thất bại
        }

        // --- 2. ĐĂNG KÝ ADMIN (Tạo tài khoản Admin mới) ---
        public bool RegisterAdmin(string username, string password, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    // BƯỚC 1: Kiểm tra tài khoản tồn tại chưa
                    string checkSql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            message = "Tên đăng nhập đã tồn tại.";
                            return false;
                        }
                    }

                    // BƯỚC 2: Đảm bảo Role 'Admin' có tồn tại trong bảng Roles
                    // (Phòng trường hợp chạy code lần đầu chưa có dữ liệu mẫu)
                    EnsureRoleExists(conn, 1, "Admin");

                    // BƯỚC 3: Thêm User mới với RoleID = 1 (Admin)
                    string insertSql = @"
                        INSERT INTO Users (Username, Password, FullName, RoleID, CreatedAt) 
                        VALUES (@Username, @Password, N'Administrator', 1, GETDATE())";

                    using (SqlCommand insertCmd = new SqlCommand(insertSql, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@Username", username);
                        insertCmd.Parameters.AddWithValue("@Password", password);

                        int rows = insertCmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            message = "Đăng ký Admin thành công.";
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = "Lỗi kết nối: " + ex.Message;
                    return false;
                }
            }
            message = "Lỗi không xác định";
            return false;
        }

        // Hàm phụ: Tự động thêm Role vào DB nếu chưa có (Tránh lỗi khóa ngoại)
        private void EnsureRoleExists(SqlConnection conn, int roleId, string roleName)
        {
            string sqlCheck = "SELECT COUNT(*) FROM Roles WHERE RoleID = @ID";
            using (SqlCommand cmd = new SqlCommand(sqlCheck, conn))
            {
                cmd.Parameters.AddWithValue("@ID", roleId);
                if ((int)cmd.ExecuteScalar() == 0)
                {
                    string sqlInsert = "INSERT INTO Roles (RoleID, RoleName) VALUES (@ID, @Name)";
                    using (SqlCommand insertCmd = new SqlCommand(sqlInsert, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@ID", roleId);
                        insertCmd.Parameters.AddWithValue("@Name", roleName);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}