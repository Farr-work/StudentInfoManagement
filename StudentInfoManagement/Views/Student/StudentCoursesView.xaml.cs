using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq; // Cần thiết cho các thao tác với List/Enumerable
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace StudentInfoManagement.Views.Student
{
    // Lớp mô hình để ánh xạ dữ liệu từ SQL (Giữ nguyên)
    public class CourseDisplayModel
    {
        public string MaHocPhan { get; set; }
        public string TenMonHoc { get; set; }
        public int TinChi { get; set; }
        public string GiangVien { get; set; }
        public string Khoa { get; set; }
    }

    public partial class StudentCoursesView : UserControl
    {
        // Chuỗi kết nối của bạn (Giữ nguyên)
        private const string ConnectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        // *************** Biến lưu trữ ID Khoa (nếu ComboBox dùng DepartmentID làm giá trị) ***************
        // Nếu ComboBoxItem Content là tên Khoa, bạn cần một cách để lấy MaKhoa (DepartmentID)
        // Hiện tại, tôi giả định bạn sẽ dùng Mã Khoa để lọc

        public StudentCoursesView()
        {
            InitializeComponent();

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                // Gọi hàm LoadCourses để tải dữ liệu ban đầu
                LoadCourses();
            }));
        }

        // =========================================================================
        // HÀM TẢI DỮ LIỆU CHUNG (Hợp nhất Tải ban đầu, Tìm kiếm và Lọc)
        // =========================================================================

        private void LoadCourses()
        {
            List<CourseDisplayModel> courses = new List<CourseDisplayModel>();

            // 1. Thu thập từ khóa tìm kiếm (từ TextBox)
            string searchKeyword = txtSearch.Text.Trim();

            // 2. Thu thập điều kiện lọc Khoa (từ ComboBox)
            // Lấy DepartmentID từ ComboBoxItem (Giả định Content của ComboBoxItem là DepartmentName)
            // Cần lấy DepartmentID tương ứng để lọc
            string selectedDepartmentName = (cboDepartmentFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
            string departmentFilter = string.Empty;

            // Xử lý trường hợp ComboBoxItem không phải là "Tất cả các khoa"
            if (selectedDepartmentName != "Tất cả các khoa" && !string.IsNullOrEmpty(selectedDepartmentName))
            {
                // Dùng Tên Khoa để lọc. Lý tưởng là dùng DepartmentID, nhưng nếu chỉ có Tên Khoa:
                // Ta sẽ dùng Tên Khoa để tạo điều kiện WHERE trong SQL.
                departmentFilter = selectedDepartmentName;
            }

            // Xây dựng câu truy vấn SQL cơ bản với tất cả các JOIN cần thiết
            string sqlQuery = @"
                SELECT
                    S.SubjectID AS MaHocPhan, 
                    S.SubjectName AS TenMonHoc, 
                    S.Credits AS TinChi,
                    L.LecturerName AS GiangVien,
                    D.DepartmentName AS Khoa
                FROM SUBJECTS S
                -- Cần JOIN SECTIONS và LECTURERS để lấy GiangVien
                JOIN SECTIONS SEC ON S.SubjectID = SEC.SubjectID
                JOIN LECTURERS L ON SEC.LecturerID = L.LecturerID
                -- Cần JOIN DEPARTMENTS để lấy Tên Khoa
                JOIN DEPARTMENTS D ON S.DepartmentID = D.DepartmentID
                WHERE 1 = 1 
            ";

            List<SqlParameter> parameters = new List<SqlParameter>();

            // 3. Thêm điều kiện tìm kiếm TỪ KHÓA ĐỘNG
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                // Thêm điều kiện AND cho tìm kiếm
                sqlQuery += " AND (S.SubjectID LIKE @Keyword OR S.SubjectName LIKE @Keyword) ";
                parameters.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar) { Value = $"%{searchKeyword}%" });
            }

            // 4. Thêm điều kiện LỌC KHOA ĐỘNG
            if (!string.IsNullOrEmpty(departmentFilter))
            {
                // Thêm điều kiện AND cho lọc Khoa (lọc theo DepartmentName)
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
                        command.Parameters.AddRange(parameters.ToArray());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                courses.Add(new CourseDisplayModel
                                {
                                    MaHocPhan = reader["MaHocPhan"].ToString(),
                                    TenMonHoc = reader["TenMonHoc"].ToString(),
                                    TinChi = Convert.ToInt32(reader["TinChi"]),
                                    GiangVien = reader["GiangVien"].ToString(),
                                    Khoa = reader["Khoa"].ToString()
                                });
                            }
                        }
                    }
                }

                if (ListHocPhan != null)
                    ListHocPhan.ItemsSource = courses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách môn học: {ex.Message}", "Lỗi Database");
            }
        }

        // =========================================================================
        // HÀM XỬ LÝ SỰ KIỆN (Chỉ gọi hàm LoadCourses đã hợp nhất)
        // =========================================================================

        /// <summary>
        /// Kích hoạt việc tải lại dữ liệu khi người dùng gõ vào ô tìm kiếm.
        /// </summary>
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Tối ưu: Chỉ cần gọi hàm LoadCourses
            LoadCourses();
        }

        /// <summary>
        /// Kích hoạt việc tải lại dữ liệu khi người dùng chọn một Khoa mới.
        /// </summary>
        private void cboDepartmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Tối ưu: Chỉ cần gọi hàm LoadCourses
            // Cần kiểm tra e.AddedItems.Count > 0 để tránh lỗi khi khởi tạo lần đầu
            if (e.AddedItems.Count > 0)
            {
                LoadCourses();
            }
        }

        // Bạn có thể xóa hàm LoadCourseList() vì nó chỉ gọi LoadInitialOrSearch()
        // và bạn đã thay thế nó bằng LoadCourses() trong Constructor.
    }
}