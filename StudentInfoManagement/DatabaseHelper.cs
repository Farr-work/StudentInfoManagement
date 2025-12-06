using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace StudentInfoManagement
{
    public static class GlobalConfig
    {
        public static string CurrentUserID { get; set; } = string.Empty;
        public static string CurrentUsername { get; set; } = string.Empty;
    }

    public class DatabaseHelper
    {
        private readonly string _connectionString =
            "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

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
                                role = reader["RoleName"].ToString();
                                userId = reader["UserID"].ToString();
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

        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SQL Error: " + ex.Message);
                }
            }
            return dt;
        }

        public bool ExecuteNonQuery(string sql, Action<SqlCommand> parameterize = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        parameterize?.Invoke(cmd);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SQL Error: " + ex.Message);
                    throw ex;
                }
            }
        }

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

        public bool RegisterAdmin(string username, string password, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

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

                    EnsureRoleExists(conn, 1, "Admin");

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

        public bool ChangePassword(string username, string currentPassword, string newPassword, out string message)
        {
            message = "";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

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

                    string userId = result.ToString();

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
