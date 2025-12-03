using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StudentInfoManagement.Views
{
    // 1. Tạo Model cho Lớp học phần
    public class ClassSection
    {
        public string ClassID { get; set; }
        public string SubjectName { get; set; }
        public string Schedule { get; set; }
        public string Lecturer { get; set; }
        public int CurrentCount { get; set; }
        public int MaxCount { get; set; }
        public string Semester { get; set; } // Dùng để lọc

        // Các thuộc tính tính toán (chỉ để hiển thị)
        public string StudentCountDisplay => $"{CurrentCount}/{MaxCount}";
        public string PercentageDisplay => $"{(double)CurrentCount / MaxCount:P0}";

        // Logic màu sắc thanh tiến độ
        public Brush ProgressBarColor => IsFull ? Brushes.Red : (CurrentCount > MaxCount * 0.8 ? Brushes.Orange : Brushes.Blue);

        // Logic Trạng thái
        public bool IsFull => CurrentCount >= MaxCount;
        public string StatusText => IsFull ? "Đã đầy" : "Đang mở";
        public Brush StatusBgColor => IsFull ? (Brush)new BrushConverter().ConvertFrom("#FEE2E2") : (Brush)new BrushConverter().ConvertFrom("#DCFCE7");
        public Brush StatusFgColor => IsFull ? Brushes.DarkRed : Brushes.DarkGreen;
    }

    public partial class CoursesPortal : UserControl
    {
        // Danh sách gốc (Database giả lập)
        private List<ClassSection> _allClasses;

        public CoursesPortal()
        {
            InitializeComponent();
            LoadDummyData();
            ApplyFilters(); // Hiển thị dữ liệu lần đầu
        }

        private void LoadDummyData()
        {
            // Tạo dữ liệu mẫu
            _allClasses = new List<ClassSection>
            {
                new ClassSection { ClassID = "CS101-01", SubjectName = "Nhập môn Lập trình", Schedule = "Thứ 2, Tiết 1-3", Lecturer = "TS. Nguyễn Văn A", CurrentCount = 45, MaxCount = 50, Semester = "HK1" },
                new ClassSection { ClassID = "CS101-02", SubjectName = "Nhập môn Lập trình", Schedule = "Thứ 3, Tiết 4-6", Lecturer = "ThS. Lê Thị B", CurrentCount = 10, MaxCount = 50, Semester = "HK1" },
                new ClassSection { ClassID = "WEB202-01", SubjectName = "Lập trình Web Frontend", Schedule = "Thứ 4, Tiết 7-9", Lecturer = "TS. Phạm Văn C", CurrentCount = 60, MaxCount = 60, Semester = "HK1" }, // Đã đầy
                new ClassSection { ClassID = "DB101-01", SubjectName = "Cơ sở dữ liệu", Schedule = "Thứ 5, Tiết 1-3", Lecturer = "ThS. Trần D", CurrentCount = 30, MaxCount = 60, Semester = "HK2" },
                new ClassSection { ClassID = "ENG301-01", SubjectName = "Tiếng Anh chuyên ngành", Schedule = "Thứ 6, Tiết 1-3", Lecturer = "Ms. Sarah", CurrentCount = 55, MaxCount = 60, Semester = "HK1" },
            };
        }

        // --- LOGIC LỌC DỮ LIỆU ---
        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allClasses == null) return;

            // 1. Lấy giá trị từ các ô nhập liệu
            string keyword = txtSearch.Text.ToLower();

            // Lấy text từ ComboBox (xử lý null safe)
            string selectedSemester = (cbSemester.SelectedItem as ComboBoxItem)?.Content.ToString();
            string selectedStatus = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();

            // 2. Thực hiện lọc dùng LINQ
            var filteredList = _allClasses.Where(c =>
            {
                // Lọc theo từ khóa (Mã lớp hoặc Tên môn)
                bool matchKeyword = string.IsNullOrEmpty(keyword) ||
                                    c.ClassID.ToLower().Contains(keyword) ||
                                    c.SubjectName.ToLower().Contains(keyword);

                // Lọc theo Học kỳ
                bool matchSemester = selectedSemester == "Tất cả học kỳ" ||
                                     (selectedSemester.Contains("HK1") && c.Semester == "HK1") ||
                                     (selectedSemester.Contains("HK2") && c.Semester == "HK2");

                // Lọc theo Trạng thái
                bool matchStatus = selectedStatus == "Tất cả" ||
                                   (selectedStatus == "Đã đầy" && c.IsFull) ||
                                   (selectedStatus == "Đang mở" && !c.IsFull);

                return matchKeyword && matchSemester && matchStatus;
            }).ToList();

            // 3. Gán dữ liệu đã lọc vào ItemsControl
            icClassList.ItemsSource = filteredList;
        }

        // --- LOGIC NÚT BẤM ---
        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng Mở Lớp Mới đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Lấy Mã lớp từ thuộc tính Tag của nút bấm
            var btn = sender as Button;
            string classId = btn.Tag.ToString();
            MessageBox.Show($"Bạn đang muốn sửa lớp: {classId}", "Edit Action");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string classId = btn.Tag.ToString();

            var result = MessageBox.Show($"Bạn có chắc muốn xóa lớp {classId}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                // Xóa khỏi danh sách nguồn và cập nhật lại UI
                var itemToRemove = _allClasses.FirstOrDefault(x => x.ClassID == classId);
                if (itemToRemove != null)
                {
                    _allClasses.Remove(itemToRemove);
                    ApplyFilters(); // Refresh lại danh sách
                }
            }
        }
    }
}