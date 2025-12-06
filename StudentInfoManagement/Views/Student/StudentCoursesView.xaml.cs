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
        public string MaHocPhan { get; set; }      // SubjectID (Mã Môn)
        public string MaLopHocPhan { get; set; }   // SectionID
        public string TenMonHoc { get; set; }
        public int TinChi { get; set; }
        public string Khoa { get; set; }
        public string SiSoHienThi { get; set; }
        public int SiSoHienTai { get; set; }

        // Logic mới: Luôn cho phép đăng ký nếu môn đang mở
        public bool CoTheDangKy => true;

        // Màu nút: Xanh (Sẵn sàng)
        public Brush ButtonColor => (Brush)new BrushConverter().ConvertFrom("#2563EB");
    }

    public partial class StudentCoursesView : UserControl
    {
        private const string ConnectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";
        private string _currentStudentID;
        public StudentCoursesView()
        {
            InitializeComponent();

            // Lấy ID người dùng hiện tại khi khởi tạo View
            // Lưu ý: Đảm bảo GlobalConfig.CurrentUserID đã được gán giá trị lúc đăng nhập thành công
            _currentStudentID = GlobalConfig.CurrentUserID;

            // Nếu chưa đăng nhập hoặc lỗi, gán giá trị mặc định để tránh crash (tùy chọn)
            if (string.IsNullOrEmpty(_currentStudentID))
            {
                // MessageBox.Show("Lỗi: Không tìm thấy thông tin sinh viên đăng nhập!");
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

            // SQL: Vẫn dùng LEFT JOIN để lấy môn dù chưa có lớp
            string sqlQuery = @"
                SELECT 
                    S.SubjectID AS MaHocPhan, 
                    S.SubjectName AS TenMonHoc, 
                    ISNULL(S.Credits, 0) AS TinChi,
                    ISNULL(D.DepartmentName, 'Chưa phân khoa') AS Khoa,
                    
                    -- Lấy Section đầu tiên tìm thấy (hoặc NULL)
                    (SELECT TOP 1 SectionID FROM SECTIONS WHERE SubjectID = S.SubjectID) AS FoundSectionID,
                    
                    -- Đếm tổng số SV đã đăng ký môn này (trên tất cả các lớp của môn đó)
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
                                int maxCount = 65; // Mặc định hiển thị 65

                                courses.Add(new CourseDisplayModel
                                {
                                    MaHocPhan = reader["MaHocPhan"]?.ToString() ?? "",
                                    // Lưu ý: MaLopHocPhan có thể null, nhưng không quan trọng vì ta dùng MaHocPhan để xử lý
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

        // --- LOGIC MỚI: TỰ ĐỘNG TẠO LỚP NẾU CHƯA CÓ ---
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            // QUAN TRỌNG: Tag bây giờ chứa SubjectID (Mã Môn), không phải SectionID
            string subjectId = btn.Tag.ToString();

            if (MessageBox.Show($"Đăng ký môn {subjectId}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        string finalSectionId = "";

                        // BƯỚC 1: Tìm xem môn này đã có lớp nào chưa
                        string findSectionSql = "SELECT TOP 1 SectionID FROM SECTIONS WHERE SubjectID = @SubID";
                        using (SqlCommand findCmd = new SqlCommand(findSectionSql, conn))
                        {
                            findCmd.Parameters.AddWithValue("@SubID", subjectId);
                            var result = findCmd.ExecuteScalar();

                            if (result != null)
                            {
                                // Đã có lớp -> Lấy ID lớp đó
                                finalSectionId = result.ToString();
                            }
                            else
                            {
                                // CHƯA CÓ LỚP -> TẠO LỚP TỰ ĐỘNG
                                finalSectionId = subjectId + "_AUTO"; // Mã lớp tự sinh: IT1_AUTO
                                string createSql = @"
                                    INSERT INTO SECTIONS (SectionID, SubjectID, Semester, LecturerID, MaxCapacity)
                                    VALUES (@SecID, @SubID, N'HK1', NULL, 65)"; // Mặc định HK1, GV Null, Max 65

                                using (SqlCommand createCmd = new SqlCommand(createSql, conn))
                                {
                                    createCmd.Parameters.AddWithValue("@SecID", finalSectionId);
                                    createCmd.Parameters.AddWithValue("@SubID", subjectId);
                                    createCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // BƯỚC 2: Kiểm tra trùng đăng ký
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

                        // BƯỚC 3: Đăng ký vào bảng REGISTRATIONS
                        string insertSql = "INSERT INTO REGISTRATIONS (masv, SectionID, RegistrationDate) VALUES (@MaSV, @SecID, GETDATE())";
                        using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaSV", _currentStudentID);
                            cmd.Parameters.AddWithValue("@SecID", finalSectionId);
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Đăng ký thành công!");
                            LoadCourses(); // Cập nhật lại sĩ số
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