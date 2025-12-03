using System.Windows;
using System.Windows.Controls;

namespace StudentInfoManagement.Views
{
    public partial class SettingViews : UserControl
    {
        // Biến lưu vai trò hiện tại ("Admin" hoặc "Student")
        private string _userRole;

        public SettingViews(string role = "Admin") // Mặc định là Admin nếu không truyền gì
        {
            InitializeComponent();
            _userRole = role;

            // Thiết lập giao diện dựa trên vai trò
            SetupViewByRole();
        }

        private void SetupViewByRole()
        {
            if (_userRole == "Admin")
            {
                // === CẤU HÌNH CHO ADMIN ===
                // 1. Giao diện bên trái
                txtAvatarIcon.Text = "🛡️"; // Icon khiên bảo mật
                txtDisplayName.Text = "Administrator";
                txtDisplayRole.Text = "Quản Trị Hệ Thống";
                txtPermissionLabel.Text = "Toàn quyền (Full Access)";

                // 2. Form bên phải (Admin được sửa tất cả)
                SetFieldsReadOnly(false);
                btnSaveInfo.Visibility = Visibility.Visible; // Hiện nút lưu

                // Load dữ liệu mẫu Admin
                txtID.Text = "ADMIN001";
                txtClassDept.Text = "Phòng Đào Tạo";
                txtFullName.Text = "Nguyễn Quản Trị";
                txtEmail.Text = "admin@dtpsystem.edu.vn";
            }
            else
            {
                // === CẤU HÌNH CHO SINH VIÊN (Dùng cho sau này) ===
                // 1. Giao diện bên trái
                txtAvatarIcon.Text = "🎓"; // Icon mũ tốt nghiệp
                txtDisplayName.Text = "Nguyễn Văn A"; // Lấy từ DB
                txtDisplayRole.Text = "Sinh Viên - K15";
                txtPermissionLabel.Text = "Hạn chế (Chỉ xem)";

                // 2. Form bên phải (Sinh viên KHÔNG ĐƯỢC SỬA thông tin cá nhân)
                SetFieldsReadOnly(true);
                btnSaveInfo.Visibility = Visibility.Collapsed; // Ẩn nút lưu đi

                // Load dữ liệu mẫu Sinh viên
                txtID.Text = "SV2024102";
                txtClassDept.Text = "CNTT_K15A";
                txtFullName.Text = "Nguyễn Văn A";
                txtEmail.Text = "vana@st.dtp.edu.vn";
            }
        }

        // Hàm tiện ích để khóa/mở khóa hàng loạt TextBox
        private void SetFieldsReadOnly(bool isReadOnly)
        {
            txtID.IsReadOnly = isReadOnly;
            txtClassDept.IsReadOnly = isReadOnly;
            txtFullName.IsReadOnly = isReadOnly;
            txtDob.IsReadOnly = isReadOnly;
            txtEmail.IsReadOnly = isReadOnly;
            txtPhone.IsReadOnly = isReadOnly;
            txtAddress.IsReadOnly = isReadOnly;
        }

        private void BtnSaveInfo_Click(object sender, RoutedEventArgs e)
        {
            // Chỉ Admin mới bấm được nút này (vì Student bị ẩn nút rồi)
            MessageBox.Show($"[ADMIN MODE] Đã cập nhật thông tin cho tài khoản: {txtFullName.Text}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            // Cả Admin và Student đều dùng được
            if (pbNewPass.Password != pbConfirmPass.Password)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            pbCurrentPass.Clear();
            pbNewPass.Clear();
            pbConfirmPass.Clear();
        }
    }
}