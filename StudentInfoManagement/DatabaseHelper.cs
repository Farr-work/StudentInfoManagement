using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace StudentInfoManagement
{
    // --- GLOBAL CONFIG (Giữ lại để tránh lỗi các file khác đang dùng) ---
    public static class GlobalConfig
    {
        public static string CurrentUserID { get; set; } = string.Empty;
        // Add CurrentUsername to store the login username (for students it's masv)
        public static string CurrentUsername { get; set; } = string.Empty;
    }

    public class DatabaseHelper
    {
        // Chuỗi kết nối chuẩn
        private readonly string _connectionString =
            "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        // --- 1. XỬ LÝ ĐĂNG NHẬP (AUTH) ---
        public string AuthenticateUser(string username, string password, out string userId)
        {
            userId = "";
            string role = "";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                        SELECT r.RoleName, u.UserID 
                        FROM Users u
                        JOIN Roles r ON u.RoleID = r.RoleID
                        WHERE u.Username = @Username AND u.Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 1. Lấy Role
                                role = reader["RoleName"].ToString();

                                // 2. Lấy ID từ Database
                                userId = reader["UserID"].ToString();

                                // Cập nhật GlobalConfig:
                                GlobalConfig.CurrentUserID = userId;
                                GlobalConfig.CurrentUsername = username;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return role;
        }

        // --- 2. LẤY THÔNG TIN SINH VIÊN (GIỮ NGUYÊN) ---
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

        // --- HÀM PHỤ: ĐẢM BẢO ROLE TỒN TẠI (GIỮ NGUYÊN) ---
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

        // --- 4. CHỨC NĂNG ĐỔI MẬT KHẨU (TỐI ƯU HÓA) ---
        // SỬA: Loại bỏ logic Stored Procedure phức tạp, sử dụng logic SQL trực tiếp như SettingViews (nhưng vẫn bọc trong helper)
        public bool ChangePassword(string username, string currentPassword, string newPassword, out string message)
        {
            message = "";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    // STEP 1: Kiểm tra mật khẩu hiện tại có khớp với Username và Password cũ không.
                    // Nếu Username là MaSV thì nó sẽ khớp với cột Username trong bảng Users
                    string checkSql = "SELECT UserID FROM Users WHERE Username = @Username AND Password = @OldPass";
                    object result;

                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        checkCmd.Parameters.AddWithValue("@OldPass", currentPassword);

                        result = checkCmd.ExecuteScalar();
                    }

                    if (result == null)
                    {
                        message = "Mật khẩu hiện tại không đúng!";
                        return false;
                    }

                    string userId = result.ToString(); // Lấy UserID thực tế từ DB

                    // STEP 2: Cập nhật mật khẩu mới cho ĐÚNG UserID đó
                    string updateSql = "UPDATE Users SET Password = @NewPass WHERE UserID = @ID";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@NewPass", newPassword);
                        updateCmd.Parameters.AddWithValue("@ID", userId);

                        int rows = updateCmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            message = "Đổi mật khẩu thành công!";
                            return true;
                        }
                        else
                        {
                            message = "Không tìm thấy UserID để cập nhật.";
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = "Lỗi Database: " + ex.Message;
                    return false;
                }
            }
        }
    }
}