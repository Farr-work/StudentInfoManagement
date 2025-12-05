using System;
using System.Windows;
using System.Windows.Input;

namespace StudentInfoManagement
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseHelper _dbHelper;

        public LoginWindow()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo");
                return;
            }

            try
            {
                // Biến để hứng ID trả về
                string userId = "";

                // GỌI HÀM MỚI (Có thêm out userId)
                string role = _dbHelper.AuthenticateUser(username, password, out userId);

                if (!string.IsNullOrEmpty(role))
                {
                    // Lưu ID vào biến toàn cục để dùng ở màn hình khác
                    // (Dù trong DatabaseHelper đã gán rồi, gán lại ở đây cho chắc chắn logic)
                    GlobalConfig.CurrentUserID = userId;

                    this.Hide();

                    if (role == "Admin")
                    {
                        MainWindow adminWindow = new MainWindow();
                        adminWindow.Show();
                    }
                    else // Student
                    {
                        StudentWindow studentWindow = new StudentWindow();
                        studentWindow.Show();
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi Đăng Nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }

        private void RegisterLink_Click(object sender, MouseButtonEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}