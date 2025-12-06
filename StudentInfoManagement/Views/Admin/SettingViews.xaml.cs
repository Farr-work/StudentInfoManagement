using Microsoft.Data.SqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace StudentInfoManagement.Views
{
    public partial class SettingViews : UserControl
    {
        public SettingViews()
        {
            InitializeComponent();
            LoadAdminProfile();
        }

        private void LoadAdminProfile()
        {
            txtAvatarIcon.Text = "🛡️";
            txtDisplayName.Text = "Administrator";
            txtDisplayRole.Text = "Quản Trị Hệ Thống";
            txtPermissionLabel.Text = "Full Access";

            SetFieldsEditable(true);
            btnSaveInfo.Visibility = Visibility.Visible;

            txtID.Text = GlobalConfig.CurrentUserID;
            txtFullName.Text = "Admin User";
            txtEmail.Text = "admin@system.com";
        }

        private void SetFieldsEditable(bool isEditable)
        {
            if (txtFullName != null) txtFullName.IsReadOnly = !isEditable;
            if (txtEmail != null) txtEmail.IsReadOnly = !isEditable;
            if (txtPhone != null) txtPhone.IsReadOnly = !isEditable;
            if (txtAddress != null) txtAddress.IsReadOnly = !isEditable;
        }

        private void BtnSaveInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng cập nhật thông tin đang được hoàn thiện.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
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

            if (pbNewPass.Password.Length < 6)
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất 6 ký tự!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //  Lấy ID người đang đăng nhập từ biến toàn cục ( App.xaml )
            string currentUserId = GlobalConfig.CurrentUserID;

            if (string.IsNullOrEmpty(currentUserId))
            {
                MessageBox.Show("Lỗi phiên đăng nhập! Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check pass 
                    string checkSql = "SELECT COUNT(*) FROM Users WHERE UserID = @ID AND Password = @OldPass";

                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@ID", currentUserId);
                        checkCmd.Parameters.AddWithValue("@OldPass", pbCurrentPass.Password);

                        int count = (int)checkCmd.ExecuteScalar();

                        if (count == 0)
                        {
                            MessageBox.Show("Mật khẩu hiện tại không đúng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    // 
                    string updateSql = "UPDATE Users SET Password = @NewPass WHERE UserID = @ID";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@NewPass", pbNewPass.Password);
                        updateCmd.Parameters.AddWithValue("@ID", currentUserId);

                        updateCmd.ExecuteNonQuery();

                        MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        pbCurrentPass.Clear();
                        pbNewPass.Clear();
                        pbConfirmPass.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}