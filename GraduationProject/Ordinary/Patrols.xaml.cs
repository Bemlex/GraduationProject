using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System.IO;

namespace GraduationProject.Ordinary
{
    /// <summary>
    /// Логика взаимодействия для Patrols.xaml
    /// </summary>
    public partial class Patrols : Window
    {
        NpgsqlConnection con = new Npgsql.NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        NpgsqlCommand cmd = new NpgsqlCommand();
        private DataTable data_table_patrols;
        private class ExportColumn
        {
            public string Header { get; set; }
            public string ColumnName { get; set; }
            public double Width { get; set; }
        }
        public Patrols()
        {
            InitializeComponent();
            LoadData();
        }
        private async void LoadData()
        {
            try
            {
                await con.OpenAsync();

                string sql = @"
            SELECT 
            p.id AS patrol_id,
            p.type AS patrol_type,
            to_char(p.start_time, 'HH24:MI DD.MM.YYYY') AS start_time,
            to_char(p.end_time, 'HH24:MI DD.MM.YYYY') AS end_time,
            p.area AS area,
            pm.employee_token AS employee_token,
            e.rank AS employee_rank,
            pe.last_name AS employee_last_name,
            pe.first_name AS employee_first_name,
            pe.middle_name AS employee_middle_name,
            d.name AS division_name,
            pm.state_number_id AS state_number_id,
            s.number AS state_number,
            m.mark AS car_mark,
            mm.model AS car_model,
            c.color AS car_color
            FROM Patrols p
            LEFT JOIN Patrol_members pm ON p.id = pm.patrol_id
            LEFT JOIN Employees e ON pm.employee_token = e.token
            LEFT JOIN State_numbers s ON pm.state_number_id = s.id
            LEFT JOIN Divisions d ON e.division_id = d.id
            LEFT JOIN People pe ON e.people_id = pe.id
            LEFT JOIN Vehicle_registrations vr ON vr.state_number_id = s.id
            LEFT JOIN Cars c ON vr.car_id = c.id
            LEFT JOIN Mark_models mm ON c.mark_model_id = mm.id
            LEFT JOIN Marks m ON mm.mark_id = m.id
                        ORDER BY p.start_time DESC;";

                using (var cmd = new NpgsqlCommand(sql, con))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    data_table_patrols = new DataTable();
                    data_table_patrols.Load(reader);
                    data_table_patrols.CaseSensitive = false;
                }

                data_table_patrols.AcceptChanges();

                foreach (DataColumn col in data_table_patrols.Columns)
                {
                    col.ReadOnly = false;
                }

                data_grid_patrols.ItemsSource = data_table_patrols.DefaultView;

                // Скрытые служебные столбцы
                data_grid_patrols.Columns[5].Visibility = Visibility.Collapsed;  // employee_token
                data_grid_patrols.Columns[11].Visibility = Visibility.Collapsed; // state_number_id
                data_grid_patrols.Columns[0].Width = new DataGridLength(150, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[0].Header = new TextBlock { Text = "№ патрулирования", FontWeight = FontWeights.Bold };
                data_grid_patrols.Columns[1].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[1].Header = new TextBlock { Text = "Тип патрулирования", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[2].Width = new DataGridLength(160, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[2].Header = new TextBlock { Text = "Начало", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[3].Width = new DataGridLength(160, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[3].Header = new TextBlock { Text = "Окончание", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[4].Width = new DataGridLength(300, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[4].Header = new TextBlock { Text = "Адрес / область", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[6].Width = new DataGridLength(150, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[6].Header = new TextBlock { Text = "Звание", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[7].Width = new DataGridLength(170, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[7].Header = new TextBlock { Text = "Фамилия", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[8].Width = new DataGridLength(150, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[8].Header = new TextBlock { Text = "Имя", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[9].Width = new DataGridLength(170, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[9].Header = new TextBlock { Text = "Отчество", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[10].Width = new DataGridLength(260, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[10].Header = new TextBlock { Text = "Подразделение", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[12].Width = new DataGridLength(130, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[12].Header = new TextBlock { Text = "Гос. номер", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[13].Width = new DataGridLength(130, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[13].Header = new TextBlock { Text = "Марка", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[14].Width = new DataGridLength(130, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[14].Header = new TextBlock { Text = "Модель", FontWeight = FontWeights.Bold };

                data_grid_patrols.Columns[15].Width = new DataGridLength(120, DataGridLengthUnitType.Pixel);
                data_grid_patrols.Columns[15].Header = new TextBlock { Text = "Цвет", FontWeight = FontWeights.Bold };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
            }
        }
        private void Button_Click_edit(object sender, RoutedEventArgs e)
        {
        }
        private void Button_Click_add(object sender, RoutedEventArgs e)
        {
        }
        private async void Button_Click_delete(object sender, RoutedEventArgs e)
        {
        }
        private async void Button_Click_save(object sender, RoutedEventArgs e)
        {
        }
        private void Button_Click_export_excel(object sender, RoutedEventArgs e)
        {
            var rows = GetDisplayedPatrolRows();

            if (rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel файл (*.xlsx)|*.xlsx";
            saveDialog.FileName = "Патрулирования.xlsx";

            if (saveDialog.ShowDialog() != true)
                return;

            ExportPatrolsToExcel(saveDialog.FileName, rows);

            MessageBox.Show("Excel-файл успешно создан.");
        }

        private void ComboBox_export_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_export == null || ComboBox_export.SelectedIndex == 0)
                return;

            if (ComboBox_export.SelectedIndex == 1)
            {
                Button_Click_export_excel(sender, e);
            }
            else if (ComboBox_export.SelectedIndex == 2)
            {
                Button_Click_export_pdf(sender, e);
            }

            ComboBox_export.SelectedIndex = 0;
        }

        private void ExportPatrolsToExcel(string filePath, List<DataRowView> rows)
        {
            var columns = GetVisiblePatrolColumns();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Патрулирования");

                for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    var cell = worksheet.Cell(1, colIndex + 1);
                    cell.Value = columns[colIndex].Header;
                    cell.Style.Font.Bold = true;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int rowIndex = 2;

                foreach (DataRowView row in rows)
                {
                    for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                    {
                        worksheet.Cell(rowIndex, colIndex + 1).Value =
                            row[columns[colIndex].ColumnName]?.ToString();
                    }

                    rowIndex++;
                }

                worksheet.Columns().AdjustToContents();
                worksheet.SheetView.FreezeRows(1);

                workbook.SaveAs(filePath);
            }
        }

        private void Button_Click_export_pdf(object sender, RoutedEventArgs e)
        {
            var rows = GetDisplayedPatrolRows();

            if (rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF файл (*.pdf)|*.pdf";
            saveDialog.FileName = "Патрулирования.pdf";

            if (saveDialog.ShowDialog() != true)
                return;

            ExportPatrolsToPdf(saveDialog.FileName, rows);

            MessageBox.Show("PDF-файл успешно создан.");
        }

        private void ExportPatrolsToPdf(string filePath, List<DataRowView> rows)
        {
            var columns = GetVisiblePatrolColumns();

            Document document = new Document(PageSize.A4.Rotate(), 15, 15, 15, 15);

            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            document.Open();

            string fontPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                "arial.ttf");

            BaseFont baseFont = BaseFont.CreateFont(
                fontPath,
                BaseFont.IDENTITY_H,
                BaseFont.EMBEDDED);

            Font titleFont = new Font(baseFont, 16, Font.BOLD);
            Font headerFont = new Font(baseFont, 8, Font.BOLD);
            Font cellFont = new Font(baseFont, 7, Font.NORMAL);

            iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("Список патрулирований", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 15;
            document.Add(title);

            PdfPTable table = new PdfPTable(columns.Count);
            table.WidthPercentage = 100;
            table.SetWidths(columns.Select(column => (float)Math.Max(column.Width, 50)).ToArray());

            foreach (ExportColumn column in columns)
            {
                PdfPCell cell = CreatePdfCell(column.Header, headerFont, Element.ALIGN_CENTER);
                table.AddCell(cell);
            }

            foreach (DataRowView row in rows)
            {
                foreach (ExportColumn column in columns)
                {
                    string value = row[column.ColumnName]?.ToString() ?? "";
                    table.AddCell(CreatePdfCell(value, cellFont, Element.ALIGN_LEFT));
                }
            }

            document.Add(table);

            document.Close();
        }

        private PdfPCell CreatePdfCell(string text, Font font, int horizontalAlignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text ?? "", font));
            cell.HorizontalAlignment = horizontalAlignment;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 3;
            return cell;
        }

        private List<DataRowView> GetDisplayedPatrolRows()
        {
            return data_grid_patrols.Items
                .OfType<DataRowView>()
                .ToList();
        }

        private List<ExportColumn> GetVisiblePatrolColumns()
        {
            return data_grid_patrols.Columns
                .Where(column => column.Visibility == Visibility.Visible)
                .OrderBy(column => column.DisplayIndex)
                .Select(column => new ExportColumn
                {
                    Header = GetColumnHeader(column),
                    ColumnName = GetColumnName(column),
                    Width = column.ActualWidth
                })
                .Where(column => !string.IsNullOrWhiteSpace(column.ColumnName))
                .ToList();
        }

        private string GetColumnHeader(DataGridColumn column)
        {
            if (column.Header is TextBlock textBlock)
                return textBlock.Text;

            return column.Header?.ToString() ?? "";
        }

        private string GetColumnName(DataGridColumn column)
        {
            if (!string.IsNullOrWhiteSpace(column.SortMemberPath))
                return NormalizeColumnName(column.SortMemberPath);

            if (column is DataGridBoundColumn boundColumn &&
                boundColumn.Binding is Binding binding)
            {
                return NormalizeColumnName(binding.Path?.Path);
            }

            return "";
        }

        private string NormalizeColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return "";

            if (columnName.StartsWith("[") && columnName.EndsWith("]"))
                return columnName.Substring(1, columnName.Length - 2);

            return columnName;
        }

        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            Ordinary_menu ordinary_menu = new Ordinary_menu();
            ordinary_menu.Show();
            this.Close();
        }
        private void TextBox_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (data_table_patrols == null)
                return;

            string searchText = TextBox_search.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                data_table_patrols.DefaultView.RowFilter = "";
                return;
            }

            searchText = searchText.Replace("'", "''");

            data_table_patrols.DefaultView.RowFilter = $@"
        Convert(patrol_id, 'System.String') LIKE '%{searchText}%'
        OR patrol_type LIKE '%{searchText}%'
        OR start_time LIKE '%{searchText}%'
        OR end_time LIKE '%{searchText}%'
        OR area LIKE '%{searchText}%'
        OR Convert(employee_token, 'System.String') LIKE '%{searchText}%'
        OR employee_rank LIKE '%{searchText}%'
        OR employee_last_name LIKE '%{searchText}%'
        OR employee_first_name LIKE '%{searchText}%'
        OR employee_middle_name LIKE '%{searchText}%'
        OR division_name LIKE '%{searchText}%'
        OR Convert(state_number_id, 'System.String') LIKE '%{searchText}%'
        OR state_number LIKE '%{searchText}%'
        OR car_mark LIKE '%{searchText}%'
        OR car_model LIKE '%{searchText}%'
        OR car_color LIKE '%{searchText}%'
    ";
        }
    }
}
