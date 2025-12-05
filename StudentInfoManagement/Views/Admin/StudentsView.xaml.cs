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
        // THAY ĐỔI CẦN THIẾT: Đảm bảo chuỗi kết nối của bạn là chính xác
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";
        private string _currentEditingMasv = null;

        public StudentsView()
        {
            InitializeComponent();
            LoadData();
        }

        #region DATABASE & LOGIC CORE

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

        // HÀM UPDATE
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

        // HÀM TẠO TÀI KHOẢN USER CHO SINH VIÊN (Username là MaSV, Password là 123)
        private bool CreateUserForStudent(string masv, string hoten, out string message)
        {
            // Cần kiểm tra trong bảng Roles của bạn. Giả sử RoleID = 2 là vai trò cho Sinh viên.
            const int studentRoleId = 2;
            const string defaultPassword = "123";

            // Lưu ý: Trong thực tế, cần HASH mật khẩu trước khi lưu
            string sql = "INSERT INTO Users (Username, Password, FullName, RoleID, CreatedAt) VALUES (@Username, @Password, @FullName, @RoleID, GETDATE())";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", masv); // Đặt Username là MaSV
                        cmd.Parameters.AddWithValue("@Password", defaultPassword); // Đặt Password mặc định là 123
                        cmd.Parameters.AddWithValue("@FullName", hoten);
                        cmd.Parameters.AddWithValue("@RoleID", studentRoleId);

                        cmd.ExecuteNonQuery();
                    }
                    message = "Tạo tài khoản người dùng thành công (Password: 123).";
                    return true;
                }
                catch (SqlException ex)
                {
                    // Lỗi khi trùng Username/UserID (Masv đã tồn tại trong Users)
                    if (ex.Number == 2627) message = "Lỗi: Tài khoản người dùng (Username: " + masv + ") đã tồn tại.";
                    else message = "Lỗi SQL khi tạo tài khoản: " + ex.Message;
                    return false;
                }
                catch (Exception ex)
                {
                    message = "Lỗi tạo tài khoản: " + ex.Message;
                    return false;
                }
            }
        }

        #endregion

        #region UI EVENTS

        // SỰ KIỆN NÚT THÊM MỚI
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _currentEditingMasv = null;
            MasvTextBox.IsEnabled = true;
            TrangthaiTextBox.IsEnabled = true;
            box.Visibility = Visibility.Visible;
            ClearInputFields();
        }

        // SỰ KIỆN NÚT HỦY
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

        // SỰ KIỆN NÚT LƯU (Bao gồm logic tạo User)
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

            string currentStatus = string.IsNullOrWhiteSpace(TrangthaiTextBox.Text) ? "Đang học" : TrangthaiTextBox.Text;

            if (_currentEditingMasv != null)
            {
                // THAO TÁC SỬA (UPDATE)
                success = UpdateStudent(masv, hoten, TenlopTextBox.Text, GioitinhTextBox.Text, DiachiTextBox.Text, EmailTextBox.Text, SdtTextBox.Text, currentStatus, out message);
            }
            else
            {
                // THAO TÁC THÊM MỚI (INSERT)
                success = InsertStudent(masv, hoten, TenlopTextBox.Text, GioitinhTextBox.Text, DiachiTextBox.Text, EmailTextBox.Text, SdtTextBox.Text, currentStatus, out message);

                if (success)
                {
                    string userMessage;
                    // Gọi hàm tạo tài khoản sau khi thêm SV thành công
                    if (CreateUserForStudent(masv, hoten, out userMessage))
                    {
                        message += "\n" + userMessage; // Thêm thông báo tạo user
                    }
                    else
                    {
                        // Cảnh báo nếu tạo User thất bại (nhưng vẫn thêm SV thành công)
                        message += "\n**CẢNH BÁO:** " + userMessage;
                    }
                }
            }

            if (success)
            {
                MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                string action = _currentEditingMasv != null
                    ? $"Cập nhật SV {masv}. Trạng thái: {currentStatus}"
                    : $"Thêm SV {masv} và tài khoản User: {masv}";
                LogActivity(action);
                box.Visibility = Visibility.Hidden;
                LoadData();
            }
            else
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // SỰ KIỆN NÚT SỬA
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
                TrangthaiTextBox.IsEnabled = true;

                box.Visibility = Visibility.Visible;
            }
        }

        // SỰ KIỆN NÚT XÓA
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

        // SỰ KIỆN TÌM KIẾM
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataView dv = StudentsGrid.ItemsSource as DataView;
            if (dv == null) return;

            string keyword = SearchTextBox.Text.Trim().Replace("'", "''");

            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    dv.RowFilter = "";
                }
                else
                {
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

        #endregion
    }
}