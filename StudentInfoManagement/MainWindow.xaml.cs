using StudentInfoManagement.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StudentInfoManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            NavigateTo("Dashboard");
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string viewName = button.Tag.ToString();

                NavigateTo(viewName);
            }
        }

        private void NavigateTo(string viewName)
        {
            switch (viewName)
            {
                case "Dashboard":
                    MainContent.Content = new DashboardView();
                    // Tạm thời dùng TextBlock để test nếu bạn chưa tạo UserControl
                    //MainContent.Content = CreatePlaceholder("Dashboard View");
                    break;

                case "Students":
                    MainContent.Content = new StudentsView();
                    break;

                case "Courses":
                    MainContent.Content = new CoursesView();
                    break;

                case "Portal":
                    MainContent.Content = CreatePlaceholder("Student Portal View");
                    break;

                case "Settings":
                     MainContent.Content = new SettingViews();
                    break;

                case "CoursesPortal":
                    MainContent.Content = new CoursesPortal();
                    break;

                default:
                    break;
            }
        }

        private TextBlock CreatePlaceholder(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Gray
            };
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}