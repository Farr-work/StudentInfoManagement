using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.SqlClient;

namespace StudentInfoManagement.Views
{
    // Model cho Môn học với Logic hiển thị trạng thái
    public class SubjectViewModel
    {
        public string SubjectID { get; set; }
        public string SubjectName { get; set; }
        public int Credits { get; set; }
        public string DepartmentID { get; set; }
        public string DepartmentName { get; set; }

        // Trạng thái từ Database (True = Đang mở, False = Đóng)
        public bool IsActive { get; set; }

        // --- Logic hiển thị (Computed Properties) ---

        // 1. Chữ hiển thị ở cột Trạng thái
        public string StatusText => IsActive ? "Đang mở" : "Đang đóng";

        // 2. Màu nền Badge trạng thái (Xanh lá / Xám đỏ)
        public Brush StatusBgColor => IsActive ?
            (Brush)new BrushConverter().ConvertFrom("#DCFCE7") : // Xanh nhạt
            (Brush)new BrushConverter().ConvertFrom("#F3F4F6");  // Xám

        // 3. Màu chữ Badge trạng thái
        public Brush StatusFgColor => IsActive ? Brushes.DarkGreen : Brushes.Gray;

        // 4. Chữ trên nút bấm (Ngược với trạng thái hiện tại)
        public string ActionButtonText => IsActive ? "Đóng lớp" : "Mở lớp";

        // 5. Màu nút bấm (Đỏ để đóng, Xanh để mở)
        public Brush ActionButtonColor => IsActive ? Brushes.Red : Brushes.DodgerBlue;
    }

    public partial class CoursesPortal : UserControl
    {
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        private List<SubjectViewModel> _allSubjects;

        public CoursesPortal()
        {
            InitializeComponent();
            LoadDepartments();
            LoadDataFromDB();
        }

        // --- TẢI DỮ LIỆU ---
        private void LoadDataFromDB()
        {
            _allSubjects = new List<SubjectViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // SỬA CÂU SQL: Thêm s.DepartmentID vào SELECT
                    string sql = @"
                SELECT 
                    s.SubjectID, 
                    s.SubjectName, 
                    s.Credits, 
                    s.DepartmentID,  -- <-- Thêm dòng này
                    ISNULL(d.DepartmentName, 'Chưa phân khoa') AS DepartmentName,
                    ISNULL(s.IsActive, 0) AS IsActive 
                FROM SUBJECTS s
                LEFT JOIN DEPARTMENTS d ON s.DepartmentID = d.DepartmentID
                ORDER BY s.SubjectName";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _allSubjects.Add(new SubjectViewModel
                            {
                                SubjectID = reader["SubjectID"].ToString(),
                                SubjectName = reader["SubjectName"].ToString(),
                                Credits = Convert.ToInt32(reader["Credits"]),

                                // --- THÊM DÒNG NÀY ---
                                DepartmentID = reader["DepartmentID"]?.ToString(),
                                // ---------------------

                                DepartmentName = reader["DepartmentName"].ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
                }
            }
            ApplyFilters();
        }
        private void LoadDepartments()
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT DepartmentID, DepartmentName FROM DEPARTMENTS", conn);
                    da.Fill(dt);
                }
                DataRow dr = dt.NewRow();
                dr["DepartmentID"] = "ALL";
                dr["DepartmentName"] = "--- Tất cả các khoa ---";
                dt.Rows.InsertAt(dr, 0);

                cbFilterDepartment.ItemsSource = dt.DefaultView;
                cbFilterDepartment.SelectedIndex = 0;
            }
            catch { }
        }

        // --- BỘ LỌC ---
        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allSubjects == null) return;

            string keyword = txtSearch.Text.ToLower();

            // Lấy Mã khoa đang chọn từ ComboBox
            string selectedDeptID = cbFilterDepartment.SelectedValue?.ToString();

            string selectedStatus = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();

            var filteredList = _allSubjects.Where(s =>
            {
                bool matchKeyword = string.IsNullOrEmpty(keyword) ||
                                    s.SubjectID.ToLower().Contains(keyword) ||
                                    s.SubjectName.ToLower().Contains(keyword);

                // --- SỬA LẠI LOGIC LỌC KHOA ---
                // Nếu chọn "ALL" hoặc null -> Lấy hết.
                // Ngược lại -> So sánh Mã khoa của môn học (s.DepartmentID) với Mã khoa đã chọn.
                bool matchDept = string.IsNullOrEmpty(selectedDeptID) ||
                                 selectedDeptID == "ALL" ||
                                 s.DepartmentID == selectedDeptID;
                // ------------------------------

                bool matchStatus = selectedStatus == "Tất cả" ||
                                   (selectedStatus == "Đang mở" && s.IsActive) ||
                                   (selectedStatus == "Đang đóng" && !s.IsActive);

                return matchKeyword && matchDept && matchStatus;
            }).ToList();

            icClassList.ItemsSource = filteredList;
        }
        // --- XỬ LÝ NÚT BẬT/TẮT (TOGGLE) ---
        // --- XỬ LÝ NÚT BẬT/TẮT (TOGGLE) + TỰ ĐỘNG TẠO LỚP ---
        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            string subjectId = btn.Tag.ToString();

            var subject = _allSubjects.FirstOrDefault(s => s.SubjectID == subjectId);
            if (subject == null) return;

            bool newStatus = !subject.IsActive;
            string actionName = newStatus ? "Mở" : "Đóng";

            if (MessageBox.Show($"Bạn có chắc muốn {actionName} đăng ký cho môn {subject.SubjectName}?",
                                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();

                        // 1. Cập nhật trạng thái Môn học
                        string updateSql = "UPDATE SUBJECTS SET IsActive = @Status WHERE SubjectID = @Id";
                        using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Status", newStatus);
                            cmd.Parameters.AddWithValue("@Id", subjectId);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. LOGIC TỰ ĐỘNG TẠO LỚP (Nếu đang MỞ và CHƯA CÓ LỚP)
                        if (newStatus == true)
                        {
                            // Kiểm tra xem môn này đã có lớp nào trong bảng SECTIONS chưa
                            string checkSectionSql = "SELECT COUNT(*) FROM SECTIONS WHERE SubjectID = @SubId";
                            using (SqlCommand checkCmd = new SqlCommand(checkSectionSql, conn))
                            {
                                checkCmd.Parameters.AddWithValue("@SubId", subjectId);
                                int sectionCount = (int)checkCmd.ExecuteScalar();

                                // Nếu chưa có lớp nào -> Tự động tạo lớp mặc định
                                if (sectionCount == 0)
                                {
                                    string autoCreateSql = @"
                                        INSERT INTO SECTIONS (SectionID, SubjectID, Semester, LecturerID, MaxCapacity)
                                        VALUES (@SecID, @SubId, @Sem, @LecID, 65)"; // Mặc định 65 người

                                    using (SqlCommand createCmd = new SqlCommand(autoCreateSql, conn))
                                    {
                                        // Tạo mã lớp tự động: MãMôn + "_01" (VD: IT1_01)
                                        createCmd.Parameters.AddWithValue("@SecID", subjectId + "_01");
                                        createCmd.Parameters.AddWithValue("@SubId", subjectId);
                                        createCmd.Parameters.AddWithValue("@Sem", "HK1"); // Mặc định HK1

                                        // Lấy 1 giảng viên mặc định (hoặc để NULL)
                                        // Ở đây mình để NULL để tránh lỗi nếu chưa có giảng viên
                                        createCmd.Parameters.AddWithValue("@LecID", DBNull.Value);

                                        createCmd.ExecuteNonQuery();

                                        MessageBox.Show($"Đã tự động tạo lớp học phần '{subjectId}_01' để sinh viên có thể đăng ký.", "Thông báo hệ thống");
                                    }
                                }
                            }
                        }

                        LoadDataFromDB(); // Tải lại giao diện
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi cập nhật: " + ex.Message);
                    }
                }
            }
        }
    }
}