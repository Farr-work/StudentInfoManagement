using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace StudentInfoManagement.Views
{
    public partial class StudentsView : UserControl
    {
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";
        private string _currentEditingMasv = null;

        public StudentsView()
        {
            InitializeComponent();
            LoadData();
        }

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
                catch { }
            }
        }

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

        // HÀM INSERT
        public bool InsertStudent(string masv, string hoten, string tenlop, string gioitinh, string diachi, string email, string sdt, string trangthai, out string message)
        {
            string sql = "INSERT INTO Student (masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai) VALUES (@MaSV, @HoTen, @TenLop, @GioiTinh, @DiaChi, @Email, @SDT, @TrangThai)";
            return ExecuteSql(sql, masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai, out message);
        }

        // HÀM UPDATE (Đã sửa để cập nhật cả trangthai)
        public bool UpdateStudent(string masv, string hoten, string tenlop, string gioitinh, string diachi, string email, string sdt, string trangthai, out string message)
        {
            string sql = "UPDATE Student SET hoten=@HoTen, tenlop=@TenLop, gioitinh=@GioiTinh, diachi=@DiaChi, email=@Email, sdt=@SDT, trangthai=@TrangThai WHERE masv = @MaSV";
            return ExecuteSql(sql, masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai, out message);
        }

        // HÀM EXECUTE CHUNG
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

                        // Luôn truyền trạng thái vào (nếu null thì DB có thể lỗi tùy cấu hình, nên đảm bảo không null từ bên ngoài)
                        cmd.Parameters.AddWithValue("@TrangThai", trangthai ?? "Đang học");

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

        // SỰ KIỆN NÚT THÊM MỚI
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _currentEditingMasv = null;
            MasvTextBox.IsEnabled = true;
            TrangthaiTextBox.IsEnabled = true; // Cho phép nhập trạng thái
            box.Visibility = Visibility.Visible;
            ClearInputFields();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
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
            TrangthaiTextBox.Clear();
        }

        // SỰ KIỆN NÚT LƯU (Đã sửa logic truyền trạng thái)
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

            // Lấy nội dung ô trạng thái
            string currentStatus = string.IsNullOrWhiteSpace(TrangthaiTextBox.Text) ? "Đang học" : TrangthaiTextBox.Text;

            if (_currentEditingMasv != null)
            {
                // Sửa: Truyền currentStatus vào hàm Update
                success = UpdateStudent(masv, hoten, TenlopTextBox.Text, GioitinhTextBox.Text, DiachiTextBox.Text, EmailTextBox.Text, SdtTextBox.Text, currentStatus, out message);
            }
            else
            {
                // Thêm: Truyền currentStatus vào hàm Insert
                success = InsertStudent(masv, hoten, TenlopTextBox.Text, GioitinhTextBox.Text, DiachiTextBox.Text, EmailTextBox.Text, SdtTextBox.Text, currentStatus, out message);
            }

            if (success)
            {
                MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                string action = _currentEditingMasv != null ? $"Cập nhật SV {masv}. Trạng thái: {currentStatus}" : $"Thêm SV {masv}";
                LogActivity(action);
                box.Visibility = Visibility.Hidden;
                LoadData();
            }
            else
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // SỰ KIỆN NÚT SỬA (Đã mở khóa ô trạng thái)
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
                TrangthaiTextBox.Text = row["trangthai"].ToString();

                MasvTextBox.IsEnabled = false;
                TrangthaiTextBox.IsEnabled = true; // MỞ KHÓA CHO PHÉP SỬA

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

        // Thêm hàm này vào trong file .cs
        // Xử lý sự kiện khi gõ vào ô tìm kiếm
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 1. Lấy DataView từ DataGrid
            DataView dv = StudentsGrid.ItemsSource as DataView;
            if (dv == null) return;

            // 2. Lấy từ khóa và xử lý ký tự đặc biệt (dấu nháy đơn) để tránh lỗi cú pháp
            string keyword = SearchTextBox.Text.Trim().Replace("'", "''");

            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    // Nếu ô trống thì bỏ lọc (hiện tất cả)
                    dv.RowFilter = "";
                }
                else
                {
                    // 3. Tạo câu lệnh lọc cho TẤT CẢ các cột
                    // Cú pháp: Cột1 LIKE '%từ_khóa%' OR Cột2 LIKE '%từ_khóa%' ...
                    // {0} sẽ được thay thế bằng biến keyword bên dưới
                    string filterFormat = "masv LIKE '%{0}%' OR " +
                                          "hoten LIKE '%{0}%' OR " +
                                          "tenlop LIKE '%{0}%' OR " +
                                          "gioitinh LIKE '%{0}%' OR " +
                                          "diachi LIKE '%{0}%' OR " +
                                          "trangthai LIKE '%{0}%'";

                    dv.RowFilter = string.Format(filterFormat, keyword);
                }
            }
            catch (Exception ex)
            {
                // Ghi nhận lỗi nếu có (thường ít khi xảy ra với DataView)
                System.Diagnostics.Debug.WriteLine("Lỗi tìm kiếm: " + ex.Message);
            }
        }

        // SỰ KIỆN XUẤT CSV (NOTEPAD)
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = null;
                if (StudentsGrid.ItemsSource is DataView dv) dt = dv.Table;

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất.");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                foreach (DataColumn col in dt.Columns) sb.Append(col.ColumnName + ",");
                sb.AppendLine();

                foreach (DataRow row in dt.Rows)
                {
                    foreach (var item in row.ItemArray)
                        sb.Append(item.ToString().Replace(",", " ") + ",");
                    sb.AppendLine();
                }

                string tempPath = Path.GetTempFileName() + ".txt";
                File.WriteAllText(tempPath, sb.ToString(), Encoding.UTF8);

                Process.Start(new ProcessStartInfo { FileName = "notepad.exe", Arguments = tempPath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message);
            }
        }
    }
}