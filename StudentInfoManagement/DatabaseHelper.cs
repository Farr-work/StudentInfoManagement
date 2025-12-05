using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace StudentInfoManagement
{
    // --- GLOBAL CONFIG GIỐNG CODE TRƯỚC ---
    public static class GlobalConfig
    {
        // Lưu mã sinh viên sau khi đăng nhập
        public static string CurrentUserID { get; set; } = string.Empty;
    }

    public class DatabaseHelper
    {
        // Chuỗi kết nối chuẩn
        private readonly string _connectionString =
            "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        // --- 1. XỬ LÝ ĐĂNG NHẬP (AUTH) ---
        public string AuthenticateUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                        SELECT r.RoleName 
                        FROM Users u
                        JOIN Roles r ON u.RoleID = r.RoleID
                        WHERE u.Username = @Username AND u.Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        object result = cmd.ExecuteScalar();

                        // Nếu login thành công
                        if (result != null)
                        {
                            // GIỐNG CODE TRƯỚC: LƯU MÃ SV VÀO GLOBAL
                            GlobalConfig.CurrentUserID = username;

                            return result.ToString(); // Trả về RoleName
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return null;
        }

        // --- 2. GIỐNG CODE TRƯỚC: LẤY THÔNG TIN SINH VIÊN ---
        public DataTable GetStudentInfo(string studentID)
        {
            string sqlQuery = @"
                SELECT hoten, tenlop, diachi, email, sdt
                FROM Student
                WHERE masv = @StudentID";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentID", studentID);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        return dt;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        // --- 3. CHỨC NĂNG ĐĂNG KÝ ADMIN (GIỮ NGUYÊN) ---
        public bool RegisterAdmin(string username, string password, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    // B1: kiểm tra username tồn tại
                    string checkSql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        if ((int)checkCmd.ExecuteScalar() > 0)
                        {
                            message = "Tên đăng nhập đã tồn tại.";
                            return false;
                        }
                    }

                    // B2: đảm bảo role Admin tồn tại
                    EnsureRoleExists(conn, 1, "Admin");

                    // B3: thêm user mới
                    string insertSql = @"
                        INSERT INTO Users (Username, Password, FullName, RoleID, CreatedAt)
                        VALUES (@Username, @Password, N'Administrator', 1, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            message = "Đăng ký Admin thành công.";
                            return true;
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

        // --- HÀM PHỤ: ĐẢM BẢO ROLE TỒN TẠI ---
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
