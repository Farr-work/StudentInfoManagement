using StudentInfoManagement;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StudentInfoManagement.Views
{
    public partial class StudentSettingView : UserControl
    {
        private readonly DatabaseHelper _dbHelper;

        public StudentSettingView()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            LoadStudentData();
        }

        private void LoadStudentData()
        {
            // Use the login username as student identifier. For student accounts Username = masv.
            // GlobalConfig.CurrentUserID remains the numeric DB UserID for admin/password operations.
            string studentID = !string.IsNullOrEmpty(GlobalConfig.CurrentUsername) ? GlobalConfig.CurrentUsername :     GlobalConfig.CurrentUserID;

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

        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            string currentPass = pbCurrentPass.Password;
            string newPass = pbNewPass.Password;
            string confirmPass = pbConfirmPass.Password;
            string userId = GlobalConfig.CurrentUserID;

            // Validate chung
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
                bool success = _dbHelper.ChangePassword(userId, currentPass, newPass, out message);

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