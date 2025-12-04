using StudentInfoManagement.Views;
using StudentInfoManagement.Views.Student;
using System.Windows;
using System.Windows.Controls;
// using StudentInfoManagement.Views.Student; // Uncomment khi có Views

namespace StudentInfoManagement
{
    public partial class StudentWindow : Window
    {
        public StudentWindow()
        {
            InitializeComponent();
            NavigateTo("StudentDashboard");
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
                case "StudentDashboard":
                     MainContent.Content = new StudentDashboardViews();
                    break;

                case "StudentCourses":
                     MainContent.Content = new StudentCoursesView();
                    break;

                case "StudentClasses":
                     MainContent.Content = new StudentClassesView();
                    break;

                case "StudentSettings":
                     MainContent.Content = new StudentSettingView();
                    break;
            }
        }

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