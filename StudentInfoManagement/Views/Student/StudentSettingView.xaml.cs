using StudentInfoManagement;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.SqlClient; // Giữ lại vì DatabaseHelper cần

namespace StudentInfoManagement.Views
{
    public partial class StudentSettingView : UserControl
    {
        private readonly DatabaseHelper _dbHelper;

        public StudentSettingView()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();

            // Attach change-password click handler at runtime
            var changeBtn = FindChangePasswordButton(this);
            if (changeBtn != null)
            {
                changeBtn.Click += BtnChangePass_Click;
            }

            LoadStudentData();
        }

        private Button? FindChangePasswordButton(DependencyObject parent)
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Button btn)
                {
                    var content = btn.Content?.ToString()?.Trim() ?? string.Empty;
                    if (string.Equals(content, "Đổi Mật Khẩu", StringComparison.Ordinal))
                    {
                        return btn;
                    }
                }

                var result = FindChangePasswordButton(child);
                if (result != null) return result;
            }

            return null;
        }

        private void LoadStudentData()
        {
            // Use the login username as student identifier. For student accounts Username = masv.
            // GlobalConfig.CurrentUserID remains the numeric DB UserID for admin/password operations.
            string studentID = !string.IsNullOrEmpty(GlobalConfig.CurrentUsername) ? GlobalConfig.CurrentUsername : GlobalConfig.CurrentUserID;

            if (string.IsNullOrEmpty(studentID))
            {
                txtFullName.Text = "Lỗi: Chưa đăng nhập.";
                txtDisplayName.Text = "Khách";
                txtDisplayRole.Text = "Chưa Đăng nhập";
                return;
            }

            try
            {
                DataTable dt = _dbHelper.GetStudentInfo(studentID);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    // Hiển thị dữ liệu
                    txtID.Text = studentID;
                    txtFullName.Text = row["hoten"].ToString();
                    txtClassDept.Text = row["tenlop"].ToString();
                    txtEmail.Text = row["email"].ToString();
                    txtPhone.Text = row["sdt"].ToString();
                    txtAddress.Text = row["diachi"].ToString();

                    // Update UI Profile Card
                    txtDisplayName.Text = txtFullName.Text;

                    // Lấy ra tên khóa (giả sử K15)
                    string className = txtClassDept.Text;
                    string cohort = className.Contains("-") ? className.Split('-')[0].Trim() : className;
                    txtDisplayRole.Text = "Sinh viên - " + cohort;
                }
                else
                {
                    txtFullName.Text = "Lỗi: Không tìm thấy thông tin sinh viên.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin sinh viên: " + ex.Message, "Lỗi Database");
            }
        }


        // Bạn có thể thêm một hàm rỗng cho BtnSaveInfo_Click nếu bạn có nút này trong XAML
        private void BtnSaveInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng cập nhật thông tin đang được hoàn thiện.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            string currentPass = pbCurrentPass.Password;
            string newPass = pbNewPass.Password;
            string confirmPass = pbConfirmPass.Password;

            // Lấy Tên đăng nhập (Username/MaSV) để truyền cho DatabaseHelper.ChangePassword
            string currentUsername = GlobalConfig.CurrentUsername;

            // 0. Kiểm tra phiên đăng nhập
            if (string.IsNullOrEmpty(currentUsername))
            {
                MessageBox.Show("Lỗi phiên đăng nhập! Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 1. Validate
            if (string.IsNullOrEmpty(currentPass) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirmPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin mật khẩu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu mới và mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newPass.Length < 6)
            {
                MessageBox.Show("Mật khẩu mới phải tối thiểu 6 ký tự!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (currentPass == newPass)
            {
                MessageBox.Show("Mật khẩu mới phải khác mật khẩu hiện tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string message;
                // Gọi hàm ChangePassword trong DatabaseHelper (sử dụng Username)
                bool success = _dbHelper.ChangePassword(currentUsername, currentPass, newPass, out message);

                if (success)
                {
                    MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Xóa trắng ô nhập
                    pbCurrentPass.Clear();
                    pbNewPass.Clear();
                    pbConfirmPass.Clear();
                }
                else
                {
                    MessageBox.Show(message, "Lỗi Đổi mật khẩu", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống khi đổi mật khẩu: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}