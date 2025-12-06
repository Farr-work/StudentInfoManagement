using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Data.SqlClient;

namespace StudentInfoManagement.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly string _connectionString = "Data Source=SQL8011.site4now.net;Initial Catalog=db_ac1c01_qlsv;User Id=db_ac1c01_qlsv_admin;Password=qlsv123@;TrustServerCertificate=True";

        public DashboardView()
        {
            InitializeComponent();

            // Load lại dữ liệu mỗi khi View được hiển thị
            this.Loaded += DashboardView_Loaded;
        }

        private void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadStatistics();
            LoadNotifications();
            LoadActivities();
        }

        private void LoadStatistics()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    txtTotalStudent.Text = GetCount(conn, "SELECT COUNT(*) FROM Student").ToString();
                    txtStudying.Text = GetCount(conn, "SELECT COUNT(*) FROM Student WHERE trangthai = N'Đang học'").ToString();
                    txtGraduated.Text = GetCount(conn, "SELECT COUNT(*) FROM Student WHERE trangthai = N'Tốt nghiệp'").ToString();
                    txtDropout.Text = GetCount(conn, "SELECT COUNT(*) FROM Student WHERE trangthai = N'Thôi học'").ToString();
                }
                catch { /* */ }
            }
        }

        private int GetCount(SqlConnection conn, string query)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                var result = cmd.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        private void LoadNotifications()
        {
            pnlNotifications.Children.Clear();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT TOP 5 Id, Title, Content, CreatedAt FROM Notifications ORDER BY CreatedAt DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader["Title"].ToString();
                            string content = reader["Content"].ToString();
                            string date = Convert.ToDateTime(reader["CreatedAt"]).ToString("dd/MM/yyyy HH:mm");

                            var border = new Border
                            {
                                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#E5E7EB"),
                                BorderThickness = new Thickness(1),
                                CornerRadius = new CornerRadius(8),
                                Padding = new Thickness(15),
                                Margin = new Thickness(0, 0, 0, 10),
                                Background = Brushes.White,
                                Cursor = Cursors.Hand,
                                Tag = new { Title = title, Content = content, Date = date }
                            };

                            border.MouseLeftButtonDown += NotificationItem_Click;

                            var sp = new StackPanel();
                            sp.Children.Add(new TextBlock { Text = title, FontSize = 14, FontWeight = FontWeights.Medium });
                            sp.Children.Add(new TextBlock { Text = date, FontSize = 12, Foreground = Brushes.Gray, Margin = new Thickness(0, 2, 0, 0) });

                            border.Child = sp;
                            pnlNotifications.Children.Add(border);
                        }
                    }
                }
                catch { }
            }
        }

        private void LoadActivities()
        {
            pnlActivities.Children.Clear();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT TOP 5 ActionName, CreatedAt FROM ActivityLog ORDER BY CreatedAt DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string action = reader["ActionName"].ToString();
                            string time = GetTimeAgo(Convert.ToDateTime(reader["CreatedAt"]));

                            var border = new Border
                            {
                                Background = (Brush)new BrushConverter().ConvertFrom("#F9FAFB"),
                                CornerRadius = new CornerRadius(8),
                                Padding = new Thickness(15),
                                Margin = new Thickness(0, 0, 0, 10)
                            };

                            var grid = new Grid();
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                            var ellipse = new System.Windows.Shapes.Ellipse
                            {
                                Width = 10,
                                Height = 10,
                                Fill = (Brush)FindResource("GreenColor"),
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 5, 15, 0)
                            };
                            Grid.SetColumn(ellipse, 0);

                            var sp = new StackPanel();
                            sp.Children.Add(new TextBlock { Text = action, FontSize = 14, FontWeight = FontWeights.Medium, TextWrapping = TextWrapping.Wrap });
                            sp.Children.Add(new TextBlock { Text = time, FontSize = 12, Foreground = Brushes.Gray });
                            Grid.SetColumn(sp, 1);

                            grid.Children.Add(ellipse);
                            grid.Children.Add(sp);
                            border.Child = grid;

                            pnlActivities.Children.Add(border);
                        }
                    }
                }
                catch { }
            }
        }

        private void BtnAddNoti_Click(object sender, RoutedEventArgs e)
        {
            txtNotiTitle.Text = "";
            txtNotiContent.Text = "";
            OverlayInput.Visibility = Visibility.Visible;
        }

        private void BtnCancelNoti_Click(object sender, RoutedEventArgs e)
        {
            OverlayInput.Visibility = Visibility.Collapsed;
        }

        private void BtnSubmitNoti_Click(object sender, RoutedEventArgs e)
        {
            string title = txtNotiTitle.Text.Trim();
            string content = txtNotiContent.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tiêu đề và nội dung.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    string insertSql = "INSERT INTO Notifications (Title, Content, CreatedAt) VALUES (@Title, @Content, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Content", content);
                        cmd.ExecuteNonQuery();
                    }

                    string cleanSql = "DELETE FROM Notifications WHERE Id NOT IN (SELECT TOP 5 Id FROM Notifications ORDER BY CreatedAt DESC)";
                    using (SqlCommand cmd = new SqlCommand(cleanSql, conn)) cmd.ExecuteNonQuery();

                    string logSql = "INSERT INTO ActivityLog (ActionName, CreatedAt) VALUES (@Action, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(logSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Action", "Admin đã thêm thông báo: " + title);
                        cmd.ExecuteNonQuery();
                    }

                    LoadNotifications();
                    LoadActivities();
                    OverlayInput.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Đã đăng thông báo thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }

        private void NotificationItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag != null)
            {
                dynamic data = border.Tag;
                lblDetailTitle.Text = data.Title;
                lblDetailContent.Text = data.Content;
                lblDetailDate.Text = data.Date;
                OverlayDetail.Visibility = Visibility.Visible;
            }
        }

        private void BtnCloseDetail_Click(object sender, RoutedEventArgs e)
        {
            OverlayDetail.Visibility = Visibility.Collapsed;
        }

        private void OverlayDetail_Click(object sender, MouseButtonEventArgs e)
        {
            OverlayDetail.Visibility = Visibility.Collapsed;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private string GetTimeAgo(DateTime date)
        {
            TimeSpan ts = DateTime.Now - date;
            if (ts.TotalMinutes < 1) return "Vừa xong";
            if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes} phút trước";
            if (ts.TotalHours < 24) return $"{(int)ts.TotalHours} giờ trước";
            if (ts.TotalDays < 7) return $"{(int)ts.TotalDays} ngày trước";
            return date.ToString("dd/MM/yyyy");
        }
    }
}