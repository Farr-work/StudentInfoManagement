using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StudentInfoManagement.Views
{
    public partial class CoursesView : UserControl
    {
        // --- DỮ LIỆU GỢI Ý ---
        private List<string> _courseCodeSuggestions = new List<string>()
        {
            "CS101", "CS102", "CS201", "CS202",
            "ENG101", "ENG201", "BUS101", "BUS201", "MAT101", "PHY101"
        };

        private List<string> _courseNameSuggestions = new List<string>()
        {
            "Introduction to Computer Science", "Data Structures", "Algorithms",
            "Database Systems", "Operating Systems", "Software Engineering",
            "Business Fundamentals", "Marketing Principles"
        };

        private List<string> _departmentSuggestions = new List<string>()
        {
            "Computer Science", "Engineering", "Business", "Mathematics", "Physics", "Arts"
        };

        public List<StudentInfoManagement.Models.CourseData> Courses { get; set; }

        public CoursesView()
        {
            InitializeComponent();
            Courses = new List<StudentInfoManagement.Models.CourseData>();
        }

        // --- 1. XỬ LÝ AUTOCOMPLETE CHUNG ---
        private void HandleAutocomplete(ComboBox sender, KeyEventArgs e, List<string> source)
        {
            if (sender == null) return;
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter || e.Key == Key.Escape) return;

            string query = sender.Text.ToLower();
            var filtered = source.Where(x => x.ToLower().Contains(query)).ToList();

            sender.ItemsSource = filtered;
            sender.IsDropDownOpen = filtered.Count > 0 && !string.IsNullOrEmpty(query);

            // Giữ con trỏ chuột ở cuối
            var textBox = (TextBox)sender.Template.FindName("PART_EditableTextBox", sender);
            if (textBox != null) textBox.SelectionStart = textBox.Text.Length;
        }

        private void CbCourseCode_KeyUp(object sender, KeyEventArgs e) => HandleAutocomplete(sender as ComboBox, e, _courseCodeSuggestions);
        private void CbCourseName_KeyUp(object sender, KeyEventArgs e) => HandleAutocomplete(sender as ComboBox, e, _courseNameSuggestions);
        private void CbDepartment_KeyUp(object sender, KeyEventArgs e) => HandleAutocomplete(sender as ComboBox, e, _departmentSuggestions);

        // --- 2. XỬ LÝ CUỘN CHUỘT (ĐÃ TỐI ƯU TỐC ĐỘ) ---
        private void Child_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (MainScrollViewer == null) return;

            // Hệ số tốc độ: 1.0 là nhanh (chuẩn), muốn nhanh hơn nữa thì giảm xuống 0.5
            double scrollSpeed = 1.0;

            double newOffset = MainScrollViewer.VerticalOffset - (e.Delta / scrollSpeed);
            newOffset = Math.Max(0, Math.Min(MainScrollViewer.ScrollableHeight, newOffset));

            MainScrollViewer.ScrollToVerticalOffset(newOffset);
            e.Handled = true;
        }

        // --- 3. XỬ LÝ CREDITS ---
        private void BtnCreditsUp_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtCredits.Text, out int val) && val < 5)
                txtCredits.Text = (val + 1).ToString();
            else if (val >= 5) txtCredits.Text = "5";
            else txtCredits.Text = "1";
        }

        private void BtnCreditsDown_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtCredits.Text, out int val) && val > 1)
                txtCredits.Text = (val - 1).ToString();
            else txtCredits.Text = "1";
        }

        private void TxtCredits_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void TxtCredits_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtCredits.Text, out int val))
            {
                if (val < 1) txtCredits.Text = "1";
                else if (val > 5) txtCredits.Text = "5";
            }
            else txtCredits.Text = "1";
        }

        // --- 4. XỬ LÝ MODAL & EXPORT ---
        private void BtnAddCourse_Click(object sender, RoutedEventArgs e)
        {
            if (ModalAddCourse != null)
            {
                ModalAddCourse.Visibility = Visibility.Visible;
                if (cbCourseCode != null) cbCourseCode.Text = "";
                if (cbCourseName != null) cbCourseName.Text = "";
                if (cbDepartment != null) cbDepartment.Text = "Computer Science";
                if (txtCredits != null) txtCredits.Text = "3";
            }
        }

        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            if (ModalAddCourse != null) ModalAddCourse.Visibility = Visibility.Collapsed;
        }

        private void BtnSaveCourse_Click(object sender, RoutedEventArgs e)
        {
            if (ModalAddCourse != null) ModalAddCourse.Visibility = Visibility.Collapsed;
        }

        private void BtnExportCSV_Click(object sender, RoutedEventArgs e)
        {
            if (dgCourses.ItemsSource == null) { MessageBox.Show("No data!"); return; }

            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Code,Name,Credits,Dept,Sem,Year");

            foreach (var item in dgCourses.ItemsSource)
            {
                var c = item as StudentInfoManagement.Models.CourseData;
                if (c != null) csv.AppendLine($"{c.CourseCode},{c.CourseName},{c.Credits},{c.Department},{c.Semester},{c.Year}");
            }

            SaveFileDialog dlg = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "Courses.csv" };
            if (dlg.ShowDialog() == true) File.WriteAllText(dlg.FileName, csv.ToString(), Encoding.UTF8);
        }

        // Placeholder handlers
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) { }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void Button_Click(object sender, RoutedEventArgs e) { }
    }
}

namespace StudentInfoManagement.Models
{
    public class CourseData
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }
        public string Department { get; set; }
        public string Semester { get; set; }
        public string Year { get; set; }
    }
}