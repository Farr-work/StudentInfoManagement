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

        // Xử lý nút Đăng Ký
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullname.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;
            string confirmPass = txtConfirmPassword.Password;

            // 1. Kiểm tra rỗng
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Kiểm tra mật khẩu khớp
            if (password != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3. Gọi Database
            string message;
            bool isSuccess = _dbHelper.RegisterUser(fullName, username, password, out message);

            if (isSuccess)
            {
                MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Chuyển về trang Login
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "Đăng ký thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý nút "Đăng nhập" (Text Link)
        private void LoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}