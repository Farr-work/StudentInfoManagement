using System.Windows;
using System.Windows.Controls;

namespace StudentInfoManagement.Views
{
    public partial class StudentSettingView : UserControl
    {
        public StudentSettingView()
        {
            InitializeComponent();
            LoadStudentData();
        }

        private void LoadStudentData()
        {
            // Tải thông tin giả lập của sinh viên
            // Trong thực tế, bạn sẽ lấy từ biến Global hoặc Database
            txtID.Text = "2024001";
            txtClassDept.Text = "CNTT_K15A";
            txtFullName.Text = "Nguyễn Văn A";
            txtDob.Text = "15/01/2003";
            txtEmail.Text = "vana@st.dtp.edu.vn";
            txtPhone.Text = "0987654321";
            txtAddress.Text = "123 Đường Nguyễn Văn Linh, Đà Nẵng";

            // Update UI Profile Card
            txtDisplayName.Text = txtFullName.Text;
            txtDisplayRole.Text = "Sinh viên - " + txtClassDept.Text;
        }

        // Sự kiện đổi mật khẩu (Chức năng duy nhất SV dùng được ở trang này)
        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            // Validate đơn giản
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

            // Logic gọi xuống Database để đổi pass ở đây...

            MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            // Xóa trắng ô nhập
            pbCurrentPass.Clear();
            pbNewPass.Clear();
            pbConfirmPass.Clear();
        }
    }
}