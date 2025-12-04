using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace StudentInfoManagement.Views
{
    public partial class StudentsView : UserControl
    {
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();

        public StudentsView()
        {
            InitializeComponent();
            LoadData();
        }
        private class Student
                {
                    public string masv { get; set; }
                    public string hoten { get; set; }
                    public string tenlop { get; set; }
                    public string gioitinh { get; set; }
                    public string diachi { get; set; }
                    public string email { get; set; }
                    public string sdt { get; set; }
                    public string trangthai { get; set; }
                }
        private void LoadData()
        {
            try
            {
                // Lấy dữ liệu sinh viên dưới dạng DataTable
                DataTable studentsTable = _dbHelper.GetStudents();

                // Gán DataTable làm nguồn dữ liệu cho DataGrid
                // DataGrid trong WPF có thể hiển thị dữ liệu từ DataTable
                StudentsGrid.ItemsSource = studentsTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải dữ liệu sinh viên: " + ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            box.Visibility = Visibility.Visible;
            ClearInputFields();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            box.Visibility = Visibility.Hidden;
            ClearInputFields();
        }
        private void ClearInputFields()
        {
            MasvTextBox.Clear();
            HotenTextBox.Clear();
            TenlopTextBox.Clear();
            GioitinhTextBox.Clear();
            DiachiTextBox.Clear();
            EmailTextBox.Clear();
            SdtTextBox.Clear();
        }

        private void SaveStudent_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy dữ liệu từ các TextBox
            string masv = MasvTextBox.Text;
            string hoten = HotenTextBox.Text;
            string tenlop = TenlopTextBox.Text;
            string gioitinh = GioitinhTextBox.Text;
            string diachi = DiachiTextBox.Text;
            string email = EmailTextBox.Text;
            string sdt = SdtTextBox.Text;
            string trangthai = "Đang học"; // Mặc định trạng thái là "Đang học"

            // 2. Kiểm tra dữ liệu cần thiết (Ví dụ: Mã SV và Họ tên không được rỗng)
            if (string.IsNullOrWhiteSpace(masv) || string.IsNullOrWhiteSpace(hoten))
            {
                MessageBox.Show("Mã Sinh Viên và Họ tên không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Gọi hàm InsertStudent
            string message;
            bool success = _dbHelper.InsertStudent(masv, hoten, tenlop, gioitinh, diachi, email, sdt, trangthai, out message);

            // 4. Xử lý kết quả
            if (success)
            {
                MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Ẩn form và tải lại dữ liệu lưới
                box.Visibility = Visibility.Hidden;
                ClearInputFields();
                LoadData();
            }
            else
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            box.Visibility = Visibility.Hidden;
            ClearInputFields();
        }
    }
}