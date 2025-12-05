using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StudentInfoManagement.Views.Student
{
    /// <summary>
    /// Model đại diện cho một lớp học hiển thị trên giao diện
    /// </summary>
    public class LopHocDisplay
    {
        public string MaLop { get; set; }        // SubjectID
        public string TenLop { get; set; }       // SubjectName
        public string ThoiGian { get; set; }     // Semester
        public string GiangVien { get; set; }    // LecturerName
        public int SiSo { get; set; }            // Số lượng hiện tại (giả định)
        public int MaxCapacity { get; set; }     // Số lượng tối đa

        // Thuộc tính hiển thị Sĩ số / MaxCapacity
        public string SiSoHienTai => $"{SiSo}/{MaxCapacity}";

        // Giữ lại 2 thuộc tính này để XAML không bị lỗi, dù không dùng cho chức năng lúc này
        public string TrangThaiText => "Xem chi tiết";
      
    }

    public partial class StudentClassesView : UserControl
    {
        // Chuỗi kết nối SQL của bạn
        // LƯU Ý: Thay thế bằng chuỗi kết nối thực tế của bạn.
        private const string ConnectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        private ObservableCollection<LopHocDisplay> _danhSachLopGoc;
        private ObservableCollection<LopHocDisplay> _danhSachLopHienThi;

        public StudentClassesView()
        {
            InitializeComponent();
            _danhSachLopGoc = new ObservableCollection<LopHocDisplay>();
            _danhSachLopHienThi = new ObservableCollection<LopHocDisplay>();

            // Gán nguồn dữ liệu cho ItemsControl
            ListLop.ItemsSource = _danhSachLopHienThi;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Tải dữ liệu khi UserControl được load
            LoadClassesData();
        }

        /// <summary>
        /// Truy vấn dữ liệu lớp học từ cơ sở dữ liệu và lưu vào danh sách gốc.
        /// </summary>
        private void LoadClassesData()
        {
            _danhSachLopGoc.Clear();
            _danhSachLopHienThi.Clear();

            string query = @"
                SELECT
                    Sub.SubjectID,
                    Sub.SubjectName,
                    S.Semester,
                    L.LecturerName,
                    S.MaxCapacity,
                    -- Dùng 50% MaxCapacity làm sĩ số giả định, hoặc dùng 1 giá trị cố định
                    CAST(S.MaxCapacity * 0.5 AS INT) AS SiSo 
                FROM SECTIONS S
                JOIN SUBJECTS Sub ON S.SubjectID = Sub.SubjectID
                JOIN LECTURERS L ON S.LecturerID = L.LecturerID;
            ";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _danhSachLopGoc.Add(new LopHocDisplay
                                {
                                    MaLop = reader.GetString(0),
                                    TenLop = reader.GetString(1),
                                    ThoiGian = reader.GetString(2),
                                    GiangVien = reader.GetString(3),
                                    MaxCapacity = reader.GetInt32(4),
                                    SiSo = reader.GetInt32(5)
                                });
                            }
                        }
                    }
                }

                // Hiển thị tất cả dữ liệu vừa load
                foreach (var lop in _danhSachLopGoc)
                {
                    _danhSachLopHienThi.Add(lop);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu lớp học: {ex.Message}", "Lỗi kết nối CSDL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện TextChanged của TextBox tìm kiếm để lọc dữ liệu.
        /// </summary>
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterClasses();
        }

        private void FilterClasses()
        {
            string searchText = txtSearch.Text.Trim();

            _danhSachLopHienThi.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                // Nếu không có tìm kiếm, hiển thị danh sách gốc
                foreach (var lop in _danhSachLopGoc)
                {
                    _danhSachLopHienThi.Add(lop);
                }
            }
            else
            {
                // Lọc theo TenLop (Tên môn học) hoặc MaLop (Mã môn học)
                var filteredList = _danhSachLopGoc.Where(l =>
                    l.TenLop.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    l.MaLop.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();

                foreach (var lop in filteredList)
                {
                    _danhSachLopHienThi.Add(lop);
                }
            }
        }
    }
}