using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StudentInfoManagement.Views
{
    public partial class StudentsView : UserControl
    {
        private ObservableCollection<Student> _students;

        public StudentsView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _students = new ObservableCollection<Student>
            {
                new Student { masv = "SV001", hoten = "Nguyễn Văn Aa", tenlop = "CTK42", gioitinh = "Nam", diachi = "Hanoi", email = "a@example.com", sdt = "0123456789", trangthai = "Active" },
                new Student { masv = "SV002", hoten = "Trần Thị B", tenlop = "CTK42", gioitinh = "Nữ", diachi = "Hanoi", email = "b@example.com", sdt = "0987654321", trangthai = "Active" },
                new Student { masv = "SV003", hoten = "Lê Văn C", tenlop = "CTK43", gioitinh = "Nam", diachi = "HCMC", email = "c@example.com", sdt = "0912345678", trangthai = "Inactive" },
                new Student { masv = "SV004", hoten = "Phạm Thị D", tenlop = "CTK44", gioitinh = "Nữ", diachi = "Da Nang", email = "d@example.com", sdt = "0909876543", trangthai = "Active" },
                new Student { masv = "SV005", hoten = "Hoàng Văn E", tenlop = "CTK45", gioitinh = "Nam", diachi = "Hue", email = "e@example.com", sdt = "0934567890", trangthai = "Active" }
            };

            StudentsGrid.ItemsSource = _students;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            box.Visibility = Visibility.Visible;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            box.Visibility = Visibility.Hidden;
        }

        // Simple student model matching the DataGrid bindings in XAML
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
    }
}