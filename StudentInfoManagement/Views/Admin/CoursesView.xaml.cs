using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace StudentInfoManagement.Views
{
    public partial class CoursesView : UserControl
    {
        // Chuỗi kết nối
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        // Biến xác định trạng thái: false = Thêm mới, true = Chỉnh sửa
        private bool _isEditMode = false;

        public CoursesView()
        {
            InitializeComponent();

            try
            {
                LoadData();
                LoadDepartments();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo: " + ex.Message);
            }
        }

        // --- 1. TẢI DỮ LIỆU ---
        private void LoadData()
        {
            string sql = @"
                SELECT 
                    s.SubjectID AS CourseCode, 
                    s.SubjectName AS CourseName, 
                    s.Credits, 
                    s.DepartmentID,
                    s.Semester,
                    d.DepartmentName AS Department
                FROM SUBJECTS s
                LEFT JOIN DEPARTMENTS d ON s.DepartmentID = d.DepartmentID";

            DataTable dt = GetDataTable(sql);
            if (dgCourses != null)
            {
                dgCourses.ItemsSource = dt.DefaultView;
            }
        }

        private void LoadDepartments()
        {
            if (cbDepartment == null || cbFilterDepartment == null) return;

            string sql = "SELECT DepartmentID, DepartmentName FROM DEPARTMENTS";
            DataTable dt = GetDataTable(sql);

            // Gán cho ComboBox trong Modal Thêm/Sửa
            cbDepartment.ItemsSource = dt.DefaultView;

            // Gán cho ComboBox Lọc
            DataTable dtFilter = dt.Copy();
            DataRow row = dtFilter.NewRow();
            row["DepartmentID"] = "ALL";
            row["DepartmentName"] = "--- Tất cả các khoa ---";
            dtFilter.Rows.InsertAt(row, 0);

            cbFilterDepartment.ItemsSource = dtFilter.DefaultView;
            cbFilterDepartment.SelectedIndex = 0;
        }

        private DataTable GetDataTable(string sql)
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
                    Debug.WriteLine("SQL Error: " + ex.Message);
                }
            }
            return dt;
        }

        // --- 2. LOGIC LỌC DỮ LIỆU ---
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { ApplyFilters(); }
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e) { ApplyFilters(); }

        private void ApplyFilters()
        {
            if (dgCourses == null || dgCourses.ItemsSource == null) return;
            DataView dv = dgCourses.ItemsSource as DataView;
            if (dv == null) return;

            string keyword = txtSearch != null ? txtSearch.Text.Trim().Replace("'", "''") : "";
            string deptId = cbFilterDepartment != null ? cbFilterDepartment.SelectedValue?.ToString() : "ALL";
            string semester = "";

            if (cbFilterSemester != null && cbFilterSemester.SelectedItem is ComboBoxItem item)
            {
                semester = item.Content.ToString();
            }

            string filter = "1=1";

            if (!string.IsNullOrEmpty(keyword))
                filter += $" AND (CourseCode LIKE '%{keyword}%' OR CourseName LIKE '%{keyword}%')";

            if (!string.IsNullOrEmpty(deptId) && deptId != "ALL")
                filter += $" AND DepartmentID = '{deptId}'";

            if (!string.IsNullOrEmpty(semester) && semester != "All Semesters")
                filter += $" AND Semester LIKE '%{semester}%'";

            try { dv.RowFilter = filter; } catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        // --- 3. CÁC SỰ KIỆN NÚT BẤM ---

        // Mở Modal để THÊM MỚI
        private void BtnOpenAddModal_Click(object sender, RoutedEventArgs e)
        {
            if (ModalAddCourse == null) return;

            // QUAN TRỌNG: Đặt lại trạng thái là Thêm mới
            _isEditMode = false;

            // Xóa trắng form và MỞ KHÓA ô Mã môn học
            if (txtCourseCode != null)
            {
                txtCourseCode.Text = "";
                txtCourseCode.IsEnabled = true; // Cho phép nhập
            }

            if (txtCourseName != null) txtCourseName.Text = "";
            if (txtCredits != null) txtCredits.Text = "3";
            if (cbDepartment != null) cbDepartment.SelectedIndex = -1;
            if (cbSemester != null) cbSemester.Text = "";

            ModalAddCourse.Visibility = Visibility.Visible;
        }

        // Mở Modal để SỬA (Mới thêm)
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ModalAddCourse == null) return;

            // Lấy dòng dữ liệu từ nút bấm
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                // 1. Chuyển sang chế độ Edit
                _isEditMode = true;

                // 2. Điền dữ liệu cũ vào các ô
                if (txtCourseCode != null)
                {
                    txtCourseCode.Text = row["CourseCode"].ToString();
                    txtCourseCode.IsEnabled = false; // KHÔNG cho sửa Mã môn học (Khóa chính)
                }

                if (txtCourseName != null)
                    txtCourseName.Text = row["CourseName"].ToString();

                if (txtCredits != null)
                    txtCredits.Text = row["Credits"].ToString();

                if (cbSemester != null)
                    cbSemester.Text = row["Semester"].ToString();

                if (cbDepartment != null)
                    cbDepartment.SelectedValue = row["DepartmentID"]; // Chọn đúng khoa

                // 3. Hiện Modal
                ModalAddCourse.Visibility = Visibility.Visible;
            }
        }

        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            if (ModalAddCourse != null)
                ModalAddCourse.Visibility = Visibility.Collapsed;
        }

        // Nút Lưu (Dùng chung cho cả Thêm và Sửa)
        private void BtnSaveCourse_Click(object sender, RoutedEventArgs e)
        {
            if (txtCourseCode == null || txtCourseName == null || cbDepartment == null || txtCredits == null) return;

            string id = txtCourseCode.Text.Trim();
            string name = txtCourseName.Text.Trim();

            // Validate dữ liệu
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập Mã và Tên môn học.");
                return;
            }

            if (cbDepartment.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Khoa.");
                return;
            }
            string deptId = cbDepartment.SelectedValue.ToString();
            string semester = cbSemester.Text.Trim();

            if (!int.TryParse(txtCredits.Text, out int credits))
            {
                MessageBox.Show("Tín chỉ phải là số.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    // --- PHÂN BIỆT LOGIC THÊM VÀ SỬA ---
                    if (_isEditMode)
                    {
                        // SỬA: Dùng lệnh UPDATE, dùng WHERE theo ID
                        cmd.CommandText = "UPDATE SUBJECTS SET SubjectName = @Name, Credits = @Cred, Semester = @Seme, DepartmentID = @Dept WHERE SubjectID = @ID";
                    }
                    else
                    {
                        // THÊM: Dùng lệnh INSERT
                        cmd.CommandText = "INSERT INTO SUBJECTS (SubjectID, SubjectName, Credits, Semester, DepartmentID) VALUES (@ID, @Name, @Cred, @Seme, @Dept)";
                    }

                    // Truyền tham số
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Cred", credits);
                    cmd.Parameters.AddWithValue("@Seme", semester);
                    cmd.Parameters.AddWithValue("@Dept", deptId);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show(_isEditMode ? "Cập nhật thành công!" : "Thêm môn học thành công!");

                    if (ModalAddCourse != null) ModalAddCourse.Visibility = Visibility.Collapsed;
                    LoadData(); // Load lại bảng
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) MessageBox.Show("Mã môn học này đã tồn tại!");
                    else MessageBox.Show("Lỗi SQL: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                string id = row["CourseCode"].ToString();
                if (MessageBox.Show($"Xóa môn {id}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        try
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM SUBJECTS WHERE SubjectID = @ID", conn);
                            cmd.Parameters.AddWithValue("@ID", id);
                            cmd.ExecuteNonQuery();
                            LoadData();
                        }
                        catch (Exception ex) { MessageBox.Show("Không thể xóa (có thể môn này đang có lớp học): " + ex.Message); }
                    }
                }
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCourses == null) return;
                DataView dv = dgCourses.ItemsSource as DataView;
                if (dv == null || dv.Count == 0) return;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Ma Mon,Ten Mon,Tin Chi,Khoa,Hoc Ky");

                foreach (DataRowView row in dv)
                {
                    string ma = row["CourseCode"]?.ToString() ?? "";
                    string ten = row["CourseName"]?.ToString() ?? "";
                    string tin = row["Credits"]?.ToString() ?? "";
                    string khoa = row["Department"]?.ToString() ?? "";
                    string ky = row["Semester"]?.ToString() ?? "";

                    sb.AppendLine($"{ma},{ten},{tin},{khoa},{ky}");
                }

                string tempFile = Path.GetTempFileName() + ".csv";
                File.WriteAllText(tempFile, sb.ToString(), Encoding.UTF8);
                Process.Start(new ProcessStartInfo { FileName = "notepad.exe", Arguments = tempFile, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message);
            }
        }
    }
}