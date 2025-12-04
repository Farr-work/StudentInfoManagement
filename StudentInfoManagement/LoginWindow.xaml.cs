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

        // Sự kiện cho nút LoginButton
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
                string role = _dbHelper.AuthenticateUser(username, password);

                if (!string.IsNullOrEmpty(role))
                {
                    this.Hide(); // Ẩn form đăng nhập

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

                    this.Close(); // Đóng hẳn khi đã chuyển trang thành công
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

        // Sự kiện cho TextBlock formdki (Chuyển sang đăng ký)
        private void RegisterLink_Click(object sender, MouseButtonEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}