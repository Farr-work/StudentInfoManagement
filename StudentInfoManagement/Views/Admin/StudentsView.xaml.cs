using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace StudentInfoManagement.Views
{
    public partial class StudentsView : UserControl
    {
        // Chuỗi kết nối đồng bộ
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        // Biến xác định đang Thêm hay Sửa (null = Thêm)
        private string _currentEditingMasv = null;

        public StudentsView()
        {
            InitializeComponent();
            LoadData();
        }

        // --- HÀM GHI NHẬT KÝ HOẠT ĐỘNG (ActivityLog) ---
        private void LogActivity(string action)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO ActivityLog (ActionName, CreatedAt) VALUES (@Action, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { /* Bỏ qua lỗi log */ }
            }
        }

        // --- CÁC HÀM XỬ LÝ DỮ LIỆU (CRUD) ---

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
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
                }
            }
            return dataTable;
        }

        private void LoadData()
        {
            DataTable dt = GetStudentsData();
            StudentsGrid.ItemsSource = dt.DefaultView;
        }

        // Thêm Sinh Viên
        public bool InsertStudent(string masv, string hoten, string tenlop, string gioitinh, string diachi, string email, string sdt, string trangthai, out string message)
        {
            string sql = "INSERT INTO Student (masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai) VALUES (@MaSV, @HoTen, @TenLop, @GioiTinh, @DiaChi, @Email, @SDT, @TrangThai)";
            return ExecuteSql(sql, masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai, out message);
        }

        // Cập nhật Sinh Viên
        public bool UpdateStudent(string masv, string hoten, string tenlop, string gioitinh, string diachi, string email, string sdt, out string message)
        {
            string sql = "UPDATE Student SET hoten=@HoTen, tenlop=@TenLop, gioitinh=@GioiTinh, diachi=@DiaChi, email=@Email, sdt=@SDT WHERE masv = @MaSV";
            return ExecuteSql(sql, masv, hoten, tenlop, gioitinh, diachi, email, sdt, null, out message);
        }

        // Hàm chung thực thi Insert/Update
        private bool ExecuteSql(string sql, string masv, string hoten, string tenlop, string gioitinh, string diachi, string email, string sdt, string trangthai, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaSV", masv);
                        cmd.Parameters.AddWithValue("@HoTen", hoten);
                        cmd.Parameters.AddWithValue("@TenLop", tenlop);
                        cmd.Parameters.AddWithValue("@GioiTinh", gioitinh);
                        cmd.Parameters.AddWithValue("@DiaChi", diachi);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@SDT", sdt);
                        if (trangthai != null)
                            cmd.Parameters.AddWithValue("@TrangThai", trangthai);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            message = "Thao tác thành công.";
                            return true;
                        }
                    }
                    message = "Không có dòng nào bị ảnh hưởng.";
                    return false;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) message = "Mã sinh viên đã tồn tại!";
                    else message = "Lỗi SQL: " + ex.Message;
                    return false;
                }
                catch (Exception ex)
                {
                    message = "Lỗi: " + ex.Message;
                    return false;
                }
            }
        }

        // Xóa Sinh Viên
        public bool DeleteStudent(string masv, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "DELETE FROM Student WHERE masv = @MaSV";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaSV", masv);
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            message = "Xóa thành công.";
                            return true;
                        }
                    }
                    message = "Không tìm thấy SV.";
                    return false;
                }
                catch (Exception ex)
                {
                    message = "Lỗi xóa: " + ex.Message;
                    return false;
                }
            }
        }

        // --- XỬ LÝ SỰ KIỆN GIAO DIỆN ---

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Nút mở form Thêm mới
            _currentEditingMasv = null;
            MasvTextBox.IsEnabled = true;
            box.Visibility = Visibility.Visible;
            ClearInputFields();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            // Nút Hủy
            box.Visibility = Visibility.Hidden;
            ClearInputFields();
        }

        private void ClearInputFields()
        {
            MasvTextBox.Clear();
            HotenTextBox.Clear();
            TenlopTextBox.Clear();
            GioitinhTextBox.Clear();
            DiachiTextBox.Clear();
            EmailTextBox.Clear();
            SdtTextBox.Clear();
        }

        private void SaveStudent_Click(object sender, RoutedEventArgs e)
        {
            string masv = MasvTextBox.Text.Trim();
            string hoten = HotenTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(masv) || string.IsNullOrWhiteSpace(hoten))
            {
                MessageBox.Show("Vui lòng nhập Mã SV và Họ tên.");
                return;
            }

            string message;
            bool success;

            if (_currentEditingMasv != null)
            {
                // Đang sửa
                success = UpdateStudent(masv, hoten, TenlopTextBox.Text, GioitinhTextBox.Text, DiachiTextBox.Text, EmailTextBox.Text, SdtTextBox.Text, out message);
            }
            else
            {
                // Đang thêm mới (Mặc định trạng thái: Đang học)
                success = InsertStudent(masv, hoten, TenlopTextBox.Text, GioitinhTextBox.Text, DiachiTextBox.Text, EmailTextBox.Text, SdtTextBox.Text, "Đang học", out message);
            }

            if (success)
            {
                MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // GHI LOG
                string action = _currentEditingMasv != null ? $"Đã cập nhật SV: {hoten}" : $"Đã thêm SV: {hoten} ({masv})";
                LogActivity(action);

                box.Visibility = Visibility.Hidden;
                LoadData();
            }
            else
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                _currentEditingMasv = row["masv"].ToString();

                MasvTextBox.Text = row["masv"].ToString();
                HotenTextBox.Text = row["hoten"].ToString();
                TenlopTextBox.Text = row["tenlop"].ToString();
                GioitinhTextBox.Text = row["gioitinh"].ToString();
                DiachiTextBox.Text = row["diachi"].ToString();
                EmailTextBox.Text = row["email"].ToString();
                SdtTextBox.Text = row["sdt"].ToString();

                MasvTextBox.IsEnabled = false; // Không cho sửa mã khi update
                box.Visibility = Visibility.Visible;
            }
        }

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                string masv = row["masv"].ToString();
                string hoten = row["hoten"].ToString();

                if (MessageBox.Show($"Xóa sinh viên {hoten}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    string msg;
                    if (DeleteStudent(masv, out msg))
                    {
                        MessageBox.Show(msg);
                        // GHI LOG XÓA
                        LogActivity($"Đã xóa SV: {hoten} ({masv})");
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show(msg);
                    }
                }
            }
        }

        // Placeholder cho nút Xuất CSV
        private void Button_Click_1(object sender, RoutedEventArgs e) { }
    }
}