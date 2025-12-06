using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Cần thêm cái này cho MouseButtonEventArgs
using System.Windows.Threading;

namespace StudentInfoManagement.Views.Student
{
    public class NotificationModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DateDisplay => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    }

    public partial class StudentDashboardViews : UserControl
    {
        private const string ConnectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";
        private string _currentStudentID;

        public StudentDashboardViews()
        {
            InitializeComponent();
            _currentStudentID = GlobalConfig.CurrentUserID;
            if (string.IsNullOrEmpty(_currentStudentID)) _currentStudentID = "SV001";

            this.Loaded += (s, e) => LoadAllData();
        }

        private void LoadAllData()
        {
            LoadStudentProfile();
            LoadNotifications();
        }

        private void LoadStudentProfile()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT hoten, tenlop FROM Student WHERE masv = @ID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", _currentStudentID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtStudentName.Text = reader["hoten"].ToString();
                                txtStudentID.Text = $"MSSV: {_currentStudentID}";
                                txtClass.Text = $"Lớp: {reader["tenlop"]}";
                            }
                            else
                            {
                                txtStudentName.Text = "Không tìm thấy sinh viên";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    txtStudentName.Text = "Lỗi kết nối";
                }
            }
        }

        private void LoadNotifications()
        {
            var list = new List<NotificationModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT TOP 20 Title, Content, CreatedAt FROM Notifications ORDER BY CreatedAt DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new NotificationModel
                            {
                                Title = reader["Title"].ToString(),
                                Content = reader["Content"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải thông báo: " + ex.Message);
                }
            }
            icNotifications.ItemsSource = list;
        }

        // --- XỬ LÝ CLICK XEM CHI TIẾT ---

        private void NotificationItem_Click(object sender, MouseButtonEventArgs e)
        {
            // Lấy Border được click
            if (sender is Border border && border.DataContext is NotificationModel noti)
            {
                // Đổ dữ liệu vào Popup
                lblDetailTitle.Text = noti.Title;
                lblDetailContent.Text = noti.Content;
                lblDetailDate.Text = noti.DateDisplay;

                // Hiện Popup
                OverlayDetail.Visibility = Visibility.Visible;
            }
        }

        private void BtnCloseDetail_Click(object sender, RoutedEventArgs e)
        {
            OverlayDetail.Visibility = Visibility.Collapsed;
        }

        // Bấm ra ngoài vùng đen để đóng
        private void Overlay_Click(object sender, MouseButtonEventArgs e)
        {
            OverlayDetail.Visibility = Visibility.Collapsed;
        }

        // Chặn sự kiện click để không bị đóng khi bấm vào nội dung trắng
        private void Popup_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}