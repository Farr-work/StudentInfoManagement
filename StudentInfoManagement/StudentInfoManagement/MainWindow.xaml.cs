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

            // Mặc định khi mở ứng dụng sẽ hiển thị Dashboard
            NavigateTo("Dashboard");
        }

        // Sự kiện xử lý khi bấm vào các nút Menu
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                // Lấy giá trị Tag (Dashboard, Students, Courses, v.v.)
                string viewName = button.Tag.ToString();

                // Gọi hàm chuyển trang
                NavigateTo(viewName);
            }
        }

        // Hàm logic để chuyển nội dung hiển thị
        private void NavigateTo(string viewName)
        {
            switch (viewName)
            {
                case "Dashboard":
                    // Ví dụ: MainContent.Content = new DashboardView();
                    // Tạm thời dùng TextBlock để test nếu bạn chưa tạo UserControl
                    MainContent.Content = CreatePlaceholder("Dashboard View");
                    break;

                case "Students":
                    MainContent.Content = new StudentsView();
                    break;

                case "Courses":
                    MainContent.Content = CreatePlaceholder("Courses List View");
                    break;

                case "Portal":
                    MainContent.Content = CreatePlaceholder("Student Portal View");
                    break;

                case "Settings":
                    // Nếu bạn đã tạo file SettingViews.xaml và sửa lỗi namespace, hãy dùng dòng dưới:
                    // MainContent.Content = new SettingViews(); 
                    MainContent.Content = CreatePlaceholder("Settings View");
                    break;

                default:
                    break;
            }
        }

        // Hàm tạo giao diện tạm thời (để code chạy được ngay mà không lỗi)
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
            // Hỏi người dùng trước khi thoát
            var result = MessageBox.Show("Bạn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown(); // Thoát ứng dụng
            }
        }
    }
}