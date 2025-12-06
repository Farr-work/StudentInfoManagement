using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StudentInfoManagement.Views.Student
{
    public class LopHocDisplay
    {
        public string MaLop { get; set; }
        public string MaLopGoc { get; set; }
        public string TenLop { get; set; }
        public string ThoiGian { get; set; }
        public string GiangVien { get; set; }
        public int SiSo { get; set; }
        public int MaxCapacity { get; set; }

        public string SiSoHienTai => $"{SiSo}/{MaxCapacity}";

        public Brush ButtonColor => (Brush)new BrushConverter().ConvertFrom("#EF4444");
    }

    public partial class StudentClassesView : UserControl
    {
        private const string ConnectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        private ObservableCollection<LopHocDisplay> _danhSachLopGoc;
        private ObservableCollection<LopHocDisplay> _danhSachLopHienThi;
        private string _currentStudentID;

        public StudentClassesView()
        {
            InitializeComponent();
            _danhSachLopGoc = new ObservableCollection<LopHocDisplay>();
            _danhSachLopHienThi = new ObservableCollection<LopHocDisplay>();
            _currentStudentID = GlobalConfig.CurrentUserID;
            ListLop.ItemsSource = _danhSachLopHienThi;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentStudentID)) _currentStudentID = "SV001";
            LoadRegisteredClasses();
        }

        private void LoadRegisteredClasses()
        {
            _danhSachLopGoc.Clear();
            _danhSachLopHienThi.Clear();

            string query = @"
                SELECT 
                    S.SectionID,
                    Sub.SubjectName,
                    S.Semester,
                    ISNULL(L.LecturerName, 'Chưa phân công') AS LecturerName,
                    ISNULL(S.MaxCapacity, 65) AS MaxCapacity,
                    (SELECT COUNT(*) FROM REGISTRATIONS CountR WHERE CountR.SectionID = S.SectionID) AS CurrentSiSo
                FROM REGISTRATIONS R
                JOIN SECTIONS S ON R.SectionID = S.SectionID
                JOIN SUBJECTS Sub ON S.SubjectID = Sub.SubjectID
                LEFT JOIN LECTURERS L ON S.LecturerID = L.LecturerID
                WHERE R.masv = @StudentID
                ORDER BY S.Semester DESC, Sub.SubjectName ASC
            ";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentID", _currentStudentID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string sectionIDRaw = reader["SectionID"].ToString();
                                string sectionIDDisplay = sectionIDRaw.Replace("_AUTO", "");

                                _danhSachLopGoc.Add(new LopHocDisplay
                                {
                                    MaLopGoc = sectionIDRaw,
                                    MaLop = sectionIDDisplay,
                                    TenLop = reader["SubjectName"].ToString(),
                                    ThoiGian = reader["Semester"].ToString(),
                                    GiangVien = reader["LecturerName"].ToString(),
                                    MaxCapacity = Convert.ToInt32(reader["MaxCapacity"]),
                                    SiSo = Convert.ToInt32(reader["CurrentSiSo"])
                                });
                            }
                        }
                    }
                }

                foreach (var lop in _danhSachLopGoc) _danhSachLopHienThi.Add(lop);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            string sectionIdToDelete = btn.Tag.ToString();

            if (MessageBox.Show($"Bạn có chắc chắn muốn hủy môn học này không?", "Xác nhận hủy", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        string sql = "DELETE FROM REGISTRATIONS WHERE masv = @MaSV AND SectionID = @SecID";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MaSV", _currentStudentID);
                            cmd.Parameters.AddWithValue("@SecID", sectionIdToDelete);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show("Đã hủy môn học thành công!", "Thông báo");
                                LoadRegisteredClasses();
                            }
                            else
                            {
                                MessageBox.Show("Không thể hủy môn học. Có thể bạn chưa đăng ký môn này.", "Lỗi");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi hệ thống: " + ex.Message);
                    }
                }
            }
        }

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
                foreach (var lop in _danhSachLopGoc) _danhSachLopHienThi.Add(lop);
            }
            else
            {
                var filteredList = _danhSachLopGoc.Where(l =>
                    l.TenLop.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    l.MaLop.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();

                foreach (var lop in filteredList) _danhSachLopHienThi.Add(lop);
            }
        }
    }
}
