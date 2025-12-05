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

        public DataTable GetStudents()
        {
            DataTable dataTable = new DataTable();
            // Lưu ý: Đảm bảo bạn đã tạo bảng 'Student' trong SQL Server
            string sqlQuery = "SELECT masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai FROM Student";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải dữ liệu sinh viên: " + ex.Message);
                }
            }
            return dataTable;
        }

        public bool InsertStudent(string masv, string hoten, string tenlop, string gioitinh, string diachi, string email, string sdt, string trangthai, out string message)
        {
            string sqlQuery = "INSERT INTO Student (masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai) VALUES (@MaSV, @HoTen, @TenLop, @GioiTinh, @DiaChi, @Email, @SDT, @TrangThai)";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaSV", masv);
                        cmd.Parameters.AddWithValue("@HoTen", hoten);
                        cmd.Parameters.AddWithValue("@TenLop", tenlop);
                        cmd.Parameters.AddWithValue("@GioiTinh", gioitinh);
                        cmd.Parameters.AddWithValue("@DiaChi", diachi);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@SDT", sdt);
                        cmd.Parameters.AddWithValue("@TrangThai", trangthai);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            message = "Thêm sinh viên mới thành công.";
                            return true;
                        }
                        else
                        {
                            message = "Không thêm được sinh viên (có thể lỗi logic SQL).";
                            return false;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Lỗi thường gặp: Trùng khóa chính (Mã SV đã tồn tại)
                    if (ex.Number == 2627)
                        message = "Mã sinh viên này đã tồn tại!";
                    else
                        message = "Lỗi SQL: " + ex.Message;

                    return false;
                }
                catch (Exception ex)
                {
                    message = "Lỗi hệ thống: " + ex.Message;
                    return false;
                }
            }
        }
    }
}