using StudentInfoManagement; // Namespace chứa DatabaseHelper và GlobalConfig
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StudentInfoManagement.Views.Student
{
    // Lớp Model cho ItemsControl (Giữ nguyên cấu trúc để XAML không lỗi)
    public class SubjectDisplayModel
    {
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public string Lecturer { get; set; }
        public string Credits { get; set; }
        public string Room { get; set; } = "TBD";
        public string ScheduleTime { get; set; } = "Thứ X, Tiết Y-Z";
    }

    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class StudentDashboardViews : UserControl
    {
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();

        public StudentDashboardViews()
        {
            InitializeComponent();

            // Dùng Dispatcher để đảm bảo control đã được load
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                // Gọi hàm quan trọng nhất
                LoadStudentProfile();

                // Gọi các hàm khác với dữ liệu giả lập/mặc định (chưa cần xử lý DB chi tiết)
                LoadCreditProgress();
                LoadCurrentSubjects();
            }));
        }

        // ===============================================
        // A. Tải thông tin Hồ sơ Sinh viên (Profile Card)
        // Dùng masv, hoten, tenlop từ GlobalConfig và DatabaseHelper
        // ===============================================
        private void LoadStudentProfile()
        {
            // Use the login username (masv) stored in CurrentUsername to fetch Student table
            string studentID = GlobalConfig.CurrentUsername;

            if (string.IsNullOrEmpty(studentID))
            {
                txtStudentName.Text = "Không xác định";
                txtStudentID.Text = "Đăng nhập lại";
                return;
            }

            try
            {
                // Dùng phương thức GetStudentInfo đã có trong DatabaseHelper
                DataTable dt = _dbHelper.GetStudentInfo(studentID);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    // Điền thông tin Profile Card (Từ bảng Student)
                    txtStudentName.Text = row["hoten"].ToString();
                    txtStudentID.Text = studentID;
                    txtClass.Text = row["tenlop"].ToString();

                }
                else
                {
                    txtStudentName.Text = "Không tìm thấy dữ liệu!";
                    txtStudentID.Text = studentID;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin cá nhân: " + ex.Message, "Lỗi Database");
            }
        }

        // ===============================================
        // B. Tải danh sách Môn học đang học (Dữ liệu giả lập)
        // ===============================================
        private void LoadCurrentSubjects()
        {

        }

        // ===============================================
        // C. Tải Tiến độ Tín chỉ (Dữ liệu giả lập)
        // ===============================================
        private void LoadCreditProgress()
        {
            int creditsEarned = 85; // Giả lập
            int creditsTotal = 130;  // Giả lập

            double progress = (double)creditsEarned / creditsTotal;
            int percent = (int)(progress * 100);

            // Cập nhật TextBlocks
            txtCreditsEarned.Text = creditsEarned.ToString();
            txtCreditsTotal.Text = creditsTotal.ToString();
            txtPercent.Text = $"Đã hoàn thành {percent}% chương trình";

            // Cập nhật ProgressBar (Giả sử chiều rộng XAML là 300)
            double maxWidth = 300;
            if (progressBarCredits != null)
            {
                progressBarCredits.Width = maxWidth * progress;
            }
        }
    }
}