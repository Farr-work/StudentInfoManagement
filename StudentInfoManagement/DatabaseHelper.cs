using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

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
        public DataTable GetStudents()
        {
            DataTable dataTable = new DataTable();
            // Thay đổi câu truy vấn SQL để lấy tất cả dữ liệu
            string sqlQuery = "SELECT masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai FROM Student";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        // Sử dụng SqlDataAdapter để điền dữ liệu vào DataTable
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Tùy chọn: Ghi log lỗi hoặc hiển thị thông báo lỗi
                    MessageBox.Show("Lỗi khi tải dữ liệu sinh viên: " + ex.Message);
                }
            }
            return dataTable;
        }
        public bool InsertStudent(string masv, string hoten, string tenlop, string gioitinh, string diachi, string email, string sdt, string trangthai, out string message)
        {
            // Cẩn thận: Chuỗi SQL này dễ bị lỗi SQL Injection.
            // Thực tế nên dùng Stored Procedure hoặc Parameterized Query.
            string sqlQuery = "INSERT INTO Student (masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai) VALUES (@MaSV, @HoTen, @TenLop, @GioiTinh, @DiaChi, @Email, @SDT, @TrangThai)";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        // Sử dụng Parameterized Query để ngăn chặn SQL Injection và xử lý dữ liệu đúng cách
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
                            message = "Không có sinh viên nào được thêm (có thể Mã SV đã tồn tại).";
                            return false;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Bắt lỗi cụ thể từ SQL (ví dụ: lỗi trùng khóa chính/Mã SV)
                    message = "Lỗi SQL khi thêm sinh viên: " + ex.Message;
                    return false;
                }
                catch (Exception ex)
                {
                    message = "Lỗi không xác định khi thêm sinh viên: " + ex.Message;
                    return false;
                }
            }
        }
    }
}