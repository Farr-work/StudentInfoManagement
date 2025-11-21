using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StudentInfoManagement.Views
{
    public partial class StudentsView : UserControl
    {
        public StudentsView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Tạo danh sách sinh viên mẫu
            List<Student> students = new List<Student>
            {
                new Student { StudentID="S001", FullName="Nguyen Van A", ClassName="10A1", Address="Hanoi", Phone="0123456789"  },
                new Student { StudentID="S002", FullName="Tran Thi B", ClassName="10A2", Address="HCMC", Phone="0987654321"},
                new Student { StudentID="S003", FullName="Le Van C", ClassName="10A3", Address="Da Nang", Phone="0912345678" }
            };

            // Gán vào DataGrid
            StudentsGrid.ItemsSource = students;
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
    }
}