using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace StudentInfoManagement.Views.Student
{
    public class CourseDisplayModel
    {
        public string MaHocPhan { get; set; }
        public string MaLopHocPhan { get; set; }
        public string TenMonHoc { get; set; }
        public int TinChi { get; set; }
        public string Khoa { get; set; }
        public string SiSoHienThi { get; set; }
        public int SiSoHienTai { get; set; }

        public bool CoTheDangKy => true;

        public Brush ButtonColor => (Brush)new BrushConverter().ConvertFrom("#2563EB");
    }

    public partial class StudentCoursesView : UserControl
    {
        private const string ConnectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";
        private string _currentStudentID;
        public StudentCoursesView()
        {
            InitializeComponent();
            _currentStudentID = GlobalConfig.CurrentUserID;

            if (string.IsNullOrEmpty(_currentStudentID))
            {
                _currentStudentID = "UNKNOWN";
            }

            this.Loaded += (s, e) => LoadCourses();
        }

        private void LoadCourses()
        {
            List<CourseDisplayModel> courses = new List<CourseDisplayModel>();
            if (txtSearch == null || cboDepartmentFilter == null) return;

            string searchKeyword = txtSearch.Text.Trim();
            string selectedDepartmentName = (cboDepartmentFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string departmentFilter = (selectedDepartmentName != "Tất cả các khoa") ? selectedDepartmentName : string.Empty;

            string sqlQuery = @"
                SELECT 
                    S.SubjectID AS MaHocPhan, 
                    S.SubjectName AS TenMonHoc, 
                    ISNULL(S.Credits, 0) AS TinChi,
                    ISNULL(D.DepartmentName, 'Chưa phân khoa') AS Khoa,
                    (SELECT TOP 1 SectionID FROM SECTIONS WHERE SubjectID = S.SubjectID) AS FoundSectionID,
                    (SELECT COUNT(*) FROM REGISTRATIONS R 
                     JOIN SECTIONS SEC ON R.SectionID = SEC.SectionID 
                     WHERE SEC.SubjectID = S.SubjectID) AS CurrentCount
                FROM SUBJECTS S
                LEFT JOIN DEPARTMENTS D ON S.DepartmentID = D.DepartmentID
                WHERE ISNULL(S.IsActive, 1) = 1 
            ";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                sqlQuery += " AND (S.SubjectID LIKE @Keyword OR S.SubjectName LIKE @Keyword) ";
                parameters.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar) { Value = $"%{searchKeyword}%" });
            }

            if (!string.IsNullOrEmpty(departmentFilter))
            {
                sqlQuery += " AND D.DepartmentName = @DepartmentName ";
                parameters.Add(new SqlParameter("@DepartmentName", SqlDbType.NVarChar) { Value = departmentFilter });
            }

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sqlQuery, connection))
                    {
                        if (parameters.Count > 0) command.Parameters.AddRange(parameters.ToArray());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int currentCount = reader["CurrentCount"] != DBNull.Value ? Convert.ToInt32(reader["CurrentCount"]) : 0;
                                int maxCount = 65;

                                courses.Add(new CourseDisplayModel
                                {
                                    MaHocPhan = reader["MaHocPhan"]?.ToString() ?? "",
                                    MaLopHocPhan = reader["FoundSectionID"]?.ToString(),
                                    TenMonHoc = reader["TenMonHoc"]?.ToString() ?? "Không tên",
                                    TinChi = reader["TinChi"] != DBNull.Value ? Convert.ToInt32(reader["TinChi"]) : 0,
                                    Khoa = reader["Khoa"]?.ToString() ?? "",
                                    SiSoHienTai = currentCount,
                                    SiSoHienThi = $"{currentCount}/{maxCount}"
                                });
                            }
                        }
                    }
                }
                ListHocPhan.ItemsSource = courses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            string subjectId = btn.Tag.ToString();

            if (MessageBox.Show($"Đăng ký môn {subjectId}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        string finalSectionId = "";

                        string findSectionSql = "SELECT TOP 1 SectionID FROM SECTIONS WHERE SubjectID = @SubID";
                        using (SqlCommand findCmd = new SqlCommand(findSectionSql, conn))
                        {
                            findCmd.Parameters.AddWithValue("@SubID", subjectId);
                            var result = findCmd.ExecuteScalar();

                            if (result != null)
                            {
                                finalSectionId = result.ToString();
                            }
                            else
                            {
                                finalSectionId = subjectId + "_AUTO";
                                string createSql = @"
                                    INSERT INTO SECTIONS (SectionID, SubjectID, Semester, LecturerID, MaxCapacity)
                                    VALUES (@SecID, @SubID, N'HK1', NULL, 65)";

                                using (SqlCommand createCmd = new SqlCommand(createSql, conn))
                                {
                                    createCmd.Parameters.AddWithValue("@SecID", finalSectionId);
                                    createCmd.Parameters.AddWithValue("@SubID", subjectId);
                                    createCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        string checkSql = "SELECT COUNT(*) FROM REGISTRATIONS WHERE masv = @MaSV AND SectionID = @SecID";
                        using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@MaSV", _currentStudentID);
                            checkCmd.Parameters.AddWithValue("@SecID", finalSectionId);
                            if ((int)checkCmd.ExecuteScalar() > 0)
                            {
                                MessageBox.Show("Bạn đã đăng ký môn này rồi!");
                                return;
                            }
                        }

                        string insertSql = "INSERT INTO REGISTRATIONS (masv, SectionID, RegistrationDate) VALUES (@MaSV, @SecID, GETDATE())";
                        using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaSV", _currentStudentID);
                            cmd.Parameters.AddWithValue("@SecID", finalSectionId);
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Đăng ký thành công!");
                            LoadCourses();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi hệ thống: " + ex.Message);
                    }
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) { LoadCourses(); }
        private void cboDepartmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) LoadCourses();
        }
    }
}
