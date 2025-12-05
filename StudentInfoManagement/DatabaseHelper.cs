using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace StudentInfoManagement
{
    // --- GLOBAL CONFIG (Giữ lại để tránh lỗi các file khác đang dùng) ---
    public static class GlobalConfig
    {
        public static string CurrentUserID { get; set; } = string.Empty;
    }

    public class DatabaseHelper
    {
        // Chuỗi kết nối chuẩn
        private readonly string _connectionString =
            "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        // --- 1. XỬ LÝ ĐĂNG NHẬP (AUTH) ---
        // SỬA: Thêm tham số "out string userId" để lấy ID ra ngoài
        public string AuthenticateUser(string username, string password, out string userId)
        {
            userId = ""; // Mặc định là rỗng
            string role = ""; // Mặc định chưa có quyền

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    // SỬA SQL: Lấy cả RoleName VÀ UserID (Giả sử cột khóa chính tên là UserID)
                    string sql = @"
                        SELECT r.RoleName, u.UserID 
                        FROM Users u
                        JOIN Roles r ON u.RoleID = r.RoleID
                        WHERE u.Username = @Username AND u.Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        // SỬA: Dùng ExecuteReader thay vì ExecuteScalar để đọc nhiều cột
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 1. Lấy Role
                                role = reader["RoleName"].ToString();

                                // 2. Lấy ID từ Database
                                // LƯU Ý: Cột trong SQL phải tên là 'UserID'. Nếu là 'Id' thì sửa dòng dưới thành ["Id"]
                                userId = reader["UserID"].ToString();

                                // Cập nhật luôn GlobalConfig cũ cho an toàn (tương thích ngược)
                                GlobalConfig.CurrentUserID = userId;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return role; // Trả về Role (Admin/Student) hoặc chuỗi rỗng nếu thất bại
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
    }
}