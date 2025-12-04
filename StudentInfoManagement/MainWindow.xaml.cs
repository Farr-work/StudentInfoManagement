using StudentInfoManagement.Views;
using System.Windows;
using System.Windows.Controls;
// using StudentInfoManagement.Views; // Uncomment khi bạn đã tạo các UserControl

namespace StudentInfoManagement
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Mặc định load trang Dashboard
            NavigateTo("Dashboard");
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                NavigateTo(button.Tag.ToString());
            }
        }

        private void NavigateTo(string viewName)
        {
            switch (viewName)
            {
                case "Dashboard":
                     MainContent.Content = new DashboardView();
                    break;

                case "Students":
                     MainContent.Content = new StudentsView();
                    break;

                case "Courses":
                     MainContent.Content = new CoursesView();
                    break;

                case "Portal":
                     MainContent.Content = new CoursesPortal();
                    break;

                case "Settings":
                     MainContent.Content = new SettingViews();
                    break;
            }
        }

        // Hàm Đăng Xuất
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }
        }
    }
}