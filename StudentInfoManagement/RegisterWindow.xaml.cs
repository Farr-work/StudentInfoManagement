using System.Windows;
using System.Windows.Input;

namespace StudentInfoManagement
{
    public partial class RegisterWindow : Window
    {
        private readonly DatabaseHelper _dbHelper;

        public RegisterWindow()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy dữ liệu từ giao diện
            // Lưu ý: txtVeriCode là tên TextBox trong XAML của bạn
            string vericode = txtVeriCode.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;
            string confirmPass = txtConfirmPassword.Password;

            // 2. Validate dữ liệu trống
            if (string.IsNullOrEmpty(vericode) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Validate Mật khẩu xác nhận
            if (password != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 4. Validate Mã định danh (Quan trọng)
            if (vericode != "2005")
            {
                MessageBox.Show("Mã định danh không đúng! Bạn không có quyền tạo tài khoản Admin.", "Truy cập bị từ chối", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 5. Gọi Database để đăng ký Admin
            string message;
            // Gọi hàm RegisterAdmin mới trong DatabaseHelper
            bool isSuccess = _dbHelper.RegisterAdmin(username, password, out message);

            if (isSuccess)
            {
                MessageBox.Show("Đăng ký Admin thành công! Vui lòng đăng nhập.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "Đăng ký thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}