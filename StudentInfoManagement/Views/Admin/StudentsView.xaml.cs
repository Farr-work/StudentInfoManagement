using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System;
using Microsoft.Data.SqlClient;

namespace StudentInfoManagement.Views
{
    public partial class StudentsView : UserControl
    {
        // QUAN TRỌNG: Chuỗi kết nối được chuyển vào đây theo yêu cầu
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        public StudentsView()
        {
            InitializeComponent();
            LoadData();
        }

        // Giữ nguyên class Student
        private class Student
        {
            public string masv { get; set; }
            public string hoten { get; set; }
            public string tenlop { get; set; }
            public string gioitinh { get; set; }
            public string diachi { get; set; }
            public string email { get; set; }
            public string sdt { get; set; }
            public string trangthai { get; set; }
        }

        // --- HÀM 1: LẤY DỮ LIỆU SINH VIÊN (CHUYỂN TỪ DatabaseHelper) ---
        private DataTable GetStudentsData()
        {
            DataTable dataTable = new DataTable();
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
                    MessageBox.Show("Lỗi khi tải dữ liệu sinh viên: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return dataTable;
        }

        private void LoadData()
        {
            try
            {
                DataTable studentsTable = GetStudentsData();
                StudentsGrid.ItemsSource = studentsTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải dữ liệu sinh viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- HÀM 2: THÊM SINH VIÊN (CHUYỂN TỪ DatabaseHelper) ---
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
                            message = "Không thêm được sinh viên (kiểm tra lại ràng buộc dữ liệu).";
                            return false;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Lỗi trùng khóa chính
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


        // --- HÀM XỬ LÝ GIAO DIỆN (ĐÃ HOÀN THIỆN) ---

        // Mở form thêm mới (Button_Click)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            box.Visibility = Visibility.Visible;
            ClearInputFields();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Chức năng chưa được xác định, giữ nguyên
        }

        // Đóng form thêm mới/Hủy bỏ (Button_Click_3)
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            box.Visibility = Visibility.Hidden;
            ClearInputFields();
        }

        private void ClearInputFields()
        {
            // Xóa nội dung các TextBox trong form thêm/sửa
            MasvTextBox.Clear();
            HotenTextBox.Clear();
            TenlopTextBox.Clear();
            GioitinhTextBox.Clear();
            DiachiTextBox.Clear();
            EmailTextBox.Clear();
            SdtTextBox.Clear();
            // Không có TrangthaiTextBox vì đã mặc định là "Đang học"
        }

        // Xử lý nút Thêm trong form modal (SaveStudent_Click)
        private void SaveStudent_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy dữ liệu từ các TextBox
            string masv = MasvTextBox.Text.Trim();
            string hoten = HotenTextBox.Text.Trim();
            string tenlop = TenlopTextBox.Text.Trim();
            string gioitinh = GioitinhTextBox.Text.Trim();
            string diachi = DiachiTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string sdt = SdtTextBox.Text.Trim();
            string trangthai = "Đang học"; // Mặc định trạng thái là "Đang học"

            // 2. Kiểm tra dữ liệu cần thiết
            if (string.IsNullOrWhiteSpace(masv) || string.IsNullOrWhiteSpace(hoten))
            {
                MessageBox.Show("Mã Sinh Viên và Họ tên không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Gọi hàm InsertStudent
            string message;
            bool success = InsertStudent(masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai, out message);

            // 4. Xử lý kết quả
            if (success)
            {
                MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Ẩn form và tải lại dữ liệu lưới
                box.Visibility = Visibility.Hidden;
                ClearInputFields();
                LoadData();
            }
            else
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý nút Sửa trong DataGrid
        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // Lấy DataRowView của hàng hiện tại
                DataRowView studentRow = button.DataContext as DataRowView;
                if (studentRow != null)
                {
                    string maSV = studentRow["masv"].ToString();
                    MessageBox.Show($"Chức năng Sửa: Bạn đang sửa sinh viên có Mã SV: {maSV}. Cần triển khai hàm UpdateStudent(masv, ...)", "Sửa Sinh Viên", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Ở đây bạn sẽ nạp dữ liệu của studentRow vào các TextBox để chỉnh sửa
                }
            }
        }

        // Xử lý nút Xóa trong DataGrid
        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // Lấy DataRowView của hàng hiện tại
                DataRowView studentRow = button.DataContext as DataRowView;
                if (studentRow != null)
                {
                    string maSV = studentRow["masv"].ToString();
                    MessageBoxResult result = MessageBox.Show($"Bạn có chắc chắn muốn xóa sinh viên {studentRow["hoten"]} ({maSV}) không?", "Xác nhận Xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        // TODO: Triển khai hàm xóa SQL và gọi LoadData()
                        MessageBox.Show($"Đã gọi chức năng Xóa cho sinh viên {maSV}. Cần triển khai hàm xóa SQL.", "Xóa Sinh Viên", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
    }
}