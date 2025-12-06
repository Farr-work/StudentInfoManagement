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
    public class SubjectViewModel
    {
        public string SubjectID { get; set; }
        public string SubjectName { get; set; }
        public int Credits { get; set; }
        public string DepartmentID { get; set; }
        public string DepartmentName { get; set; }

        public bool IsActive { get; set; }

        public string StatusText => IsActive ? "Đang mở" : "Đang đóng";

        public Brush StatusBgColor => IsActive ?
            (Brush)new BrushConverter().ConvertFrom("#DCFCE7") : // Xanh nhạt
            (Brush)new BrushConverter().ConvertFrom("#F3F4F6");  // Xám
        public Brush StatusFgColor => IsActive ? Brushes.DarkGreen : Brushes.Gray;

        public string ActionButtonText => IsActive ? "Đóng lớp" : "Mở lớp";
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
        private void LoadDataFromDB()
        {
            _allSubjects = new List<SubjectViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"
                SELECT 
                    s.SubjectID, 
                    s.SubjectName, 
                    s.Credits, 
                    s.DepartmentID,
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
                                DepartmentID = reader["DepartmentID"]?.ToString(),
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

        // --- LỌC ---
        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allSubjects == null) return;

            string keyword = txtSearch.Text.ToLower();

            string selectedDeptID = cbFilterDepartment.SelectedValue?.ToString();

            string selectedStatus = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();

            var filteredList = _allSubjects.Where(s =>
            {
                bool matchKeyword = string.IsNullOrEmpty(keyword) ||
                                    s.SubjectID.ToLower().Contains(keyword) ||
                                    s.SubjectName.ToLower().Contains(keyword);
                bool matchDept = string.IsNullOrEmpty(selectedDeptID) ||
                                 selectedDeptID == "ALL" ||
                                 s.DepartmentID == selectedDeptID;

                bool matchStatus = selectedStatus == "Tất cả" ||
                                   (selectedStatus == "Đang mở" && s.IsActive) ||
                                   (selectedStatus == "Đang đóng" && !s.IsActive);

                return matchKeyword && matchDept && matchStatus;
            }).ToList();

            icClassList.ItemsSource = filteredList;
        }
        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            string subjectId = btn.Tag.ToString();

            var subject = _allSubjects.FirstOrDefault(s => s.SubjectID == subjectId);
            if (subject == null) return;

            bool newStatus = !subject.IsActive;
            string actionName = newStatus ? "Mở" : "Đóng";

            if (MessageBox.Show($"Bạn có chắc muốn {actionName} đăng ký môn {subject.SubjectName}?",
                                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();

                        string updateSql = "UPDATE SUBJECTS SET IsActive = @Status WHERE SubjectID = @Id";
                        using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Status", newStatus);
                            cmd.Parameters.AddWithValue("@Id", subjectId);
                            cmd.ExecuteNonQuery();
                        }

                        if (newStatus == true)
                        {
                            string checkSectionSql = "SELECT COUNT(*) FROM SECTIONS WHERE SubjectID = @SubId";
                            using (SqlCommand checkCmd = new SqlCommand(checkSectionSql, conn))
                            {
                                checkCmd.Parameters.AddWithValue("@SubId", subjectId);
                                int sectionCount = (int)checkCmd.ExecuteScalar();

                                if (sectionCount == 0)
                                {
                                    string autoCreateSql = @"
                                        INSERT INTO SECTIONS (SectionID, SubjectID, Semester, LecturerID, MaxCapacity)
                                        VALUES (@SecID, @SubId, @Sem, @LecID, 65)"; // Mặc định 65 người

                                    using (SqlCommand createCmd = new SqlCommand(autoCreateSql, conn))
                                    {
                                        createCmd.Parameters.AddWithValue("@SecID", subjectId + "_01");
                                        createCmd.Parameters.AddWithValue("@SubId", subjectId);
                                        createCmd.Parameters.AddWithValue("@Sem", "HK1");
                                        createCmd.Parameters.AddWithValue("@LecID", DBNull.Value);

                                        createCmd.ExecuteNonQuery();
                                        MessageBox.Show($"Đã tự động tạo lớp học phần '{subjectId}_01' để sinh viên có thể đăng ký.", "Thông báo hệ thống");
                                    }
                                }
                            }
                        }

                        LoadDataFromDB();
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