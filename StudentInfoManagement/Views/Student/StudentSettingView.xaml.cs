using StudentInfoManagement; // Namespace chứa DatabaseHelper và GlobalConfig
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

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
            // 1. Lấy Student ID (masv) từ biến Global
            string studentID = GlobalConfig.CurrentUserID;

            if (string.IsNullOrEmpty(studentID))
            {
                txtFullName.Text = "Lỗi: Chưa đăng nhập.";
                return;
            }

            try
            {
                // 2. Gọi phương thức lấy dữ liệu
                DataTable dt = _dbHelper.GetStudentInfo(studentID);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    // 3. Hiển thị dữ liệu lên các control với tên cột mới
                    txtID.Text = studentID;

                    // Cột: hoten
                    txtFullName.Text = row["hoten"].ToString();

                    // Cột: tenlop
                    txtClassDept.Text = row["tenlop"].ToString();

                    // Cột: email
                    txtEmail.Text = row["email"].ToString();

                    // Cột: sdt
                    txtPhone.Text = row["sdt"].ToString();

                    // Cột: diachi
                    txtAddress.Text = row["diachi"].ToString();

                    // LƯU Ý: Bảng Student không có cột Ngày sinh (DoB).
                    // Nếu bạn có control txtDob, giá trị này sẽ không được điền.

                    // Update UI Profile Card
                    txtDisplayName.Text = txtFullName.Text;
                    txtDisplayRole.Text = "Sinh viên - " + txtClassDept.Text;
                }
                else
                {
                    MessageBox.Show($"Không tìm thấy thông tin sinh viên có Mã: {studentID}. Vui lòng kiểm tra bảng Student.", "Lỗi Dữ liệu");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin sinh viên: " + ex.Message, "Lỗi Database");
            }
        }

        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrEmpty(pbCurrentPass.Password) ||
                string.IsNullOrEmpty(pbNewPass.Password) ||
                string.IsNullOrEmpty(pbConfirmPass.Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (pbNewPass.Password != pbConfirmPass.Password)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ... (Logic gọi xuống Database để đổi pass ở đây) ...

            MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            // Xóa trắng ô nhập
            pbCurrentPass.Clear();
            pbNewPass.Clear();
            pbConfirmPass.Clear();
        }
    }
}