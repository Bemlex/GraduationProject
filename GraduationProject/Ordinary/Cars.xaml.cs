using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GraduationProject.Ordinary
{
    /// <summary>
    /// Логика взаимодействия для Cars.xaml
    /// </summary>
    public partial class Cars : Window
    {
        private const string ConnectionString = "Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD";
        private int loadDataRequest = 0;
        private bool isClearingPeriod = false;
        string tb_search_standart_text = "🔍 Поиск";
        public Cars()
        {
            InitializeComponent();
            LoadData();
        }
        private async void LoadData()
        {
            int requestId = ++loadDataRequest;
            List<CarsView> List_cars = new List<CarsView>();
            try
            {
                string sql = @"
SELECT
    vr.id AS registration_id,
    vr.car_id,
    m.mark AS car_mark,
    mm.model AS car_model,
    c.color AS car_color,
    vr.state_number_id,
    sn.number AS state_number,
    sn.region AS state_region,
    c.engine,
    c.release,
    c.vin,
    i.name AS insurance_name,
    owner.last_name AS owner_last_name,
    owner.first_name AS owner_first_name,
    owner.middle_name AS owner_middle_name,
    vr.date_reg,
    vr.date_end,
    vr.employee_token,
    employee_person.last_name AS employee_last_name,
    employee_person.first_name AS employee_first_name,
    employee_person.middle_name AS employee_middle_name
FROM Vehicle_registrations vr
LEFT JOIN People owner ON vr.people_id = owner.id
LEFT JOIN Cars c ON vr.car_id = c.id
LEFT JOIN Mark_models mm ON c.mark_model_id = mm.id
LEFT JOIN Marks m ON mm.mark_id = m.id
LEFT JOIN State_numbers sn ON vr.state_number_id = sn.id
LEFT JOIN Employees e ON vr.employee_token = e.token
LEFT JOIN People employee_person ON e.people_id = employee_person.id
LEFT JOIN Insurances i ON c.insurance_id = i.id";

                string searchText = Search.Search_text?.Trim();
                string[] search_parts = string.IsNullOrWhiteSpace(searchText) ? new string[0] : searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool hasSearch = searchText != tb_search_standart_text && search_parts.Length > 0;
                DateTime? dateFrom = dp_date_from.SelectedDate?.Date;
                DateTime? dateTo = dp_date_to.SelectedDate?.Date;

                if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
                {
                    MessageBox.Show("Дата начала не может быть позже даты конца");
                    return;
                }

                List<string> whereParts = new List<string>();

                if (hasSearch)
                {
                    if (dateFrom.HasValue && dateTo.HasValue)
                    {
                        whereParts.Add("(vr.date_reg <= @dateTo AND COALESCE(vr.date_end, CURRENT_DATE) >= @dateFrom)");
                    }
                    else if (dateFrom.HasValue)
                    {
                        whereParts.Add("COALESCE(vr.date_end, CURRENT_DATE) >= @dateFrom");
                    }
                    else if (dateTo.HasValue)
                    {
                        whereParts.Add("vr.date_reg <= @dateTo");
                    }
                }

                    if (dateFrom.HasValue && dateTo.HasValue)
                    whereParts.Add("(vr.date_reg <= @dateTo AND (vr.date_end IS NULL OR vr.date_end >= @dateFrom))");
                else if (dateFrom.HasValue)
                    whereParts.Add("(vr.date_end IS NULL OR vr.date_end >= @dateFrom)");
                else if (dateTo.HasValue)
                    whereParts.Add("(vr.date_reg <= @dateTo)");

                if (whereParts.Count > 0)
                    sql += " WHERE " + string.Join(" AND ", whereParts);

                sql += " ORDER BY vr.id DESC";

                using (var con = new NpgsqlConnection(ConnectionString))
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    if (hasSearch)
                    {
                        cmd.Parameters.AddWithValue("p0", $"%{search_parts[0]}%");
                        if (search_parts.Length >= 2)
                            cmd.Parameters.AddWithValue("p1", $"%{search_parts[1]}%");
                        if (search_parts.Length >= 3)
                            cmd.Parameters.AddWithValue("p2", $"%{search_parts[2]}%");
                    }

                    if (dateFrom.HasValue)
                        cmd.Parameters.AddWithValue("dateFrom", dateFrom.Value);

                    if (dateTo.HasValue)
                        cmd.Parameters.AddWithValue("dateTo", dateTo.Value);

                    await con.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            List_cars.Add(new CarsView
                            {
                                Номер_регистрации = reader["registration_id"].ToString(),
                                ID_автомобиля = reader["car_id"].ToString(),
                                Марка = reader["car_mark"].ToString(),
                                Модель = reader["car_model"].ToString(),
                                Цвет = reader["car_color"].ToString(),
                                ID_госномера = reader["state_number_id"].ToString(),
                                Государственный_номер = reader["state_number"].ToString(),
                                Регион = reader["state_region"].ToString(),
                                Двигатель = FormatEngine(reader["engine"]),
                                Год_выпуска = reader["release"].ToString(),
                                VIN = reader["vin"].ToString(),
                                Страховая_компания = reader["insurance_name"].ToString(),
                                Фамилия_владельца = reader["owner_last_name"].ToString(),
                                Имя_владельца = reader["owner_first_name"].ToString(),
                                Отчество_владельца = reader["owner_middle_name"].ToString(),
                                Дата_регистрации = reader["date_reg"] == DBNull.Value ? "" : Convert.ToDateTime(reader["date_reg"]).ToString("dd.MM.yyyy"),
                                Дата_окончания = reader["date_end"] == DBNull.Value ? "" : Convert.ToDateTime(reader["date_end"]).ToString("dd.MM.yyyy"),
                                Token_сотрудника = reader["employee_token"].ToString(),
                                Фамилия_сотрудника = reader["employee_last_name"].ToString(),
                                Имя_сотрудника = reader["employee_first_name"].ToString(),
                                Отчество_сотрудника = reader["employee_middle_name"].ToString()
                            });
                        }
                    }
                }

                if (requestId != loadDataRequest)
                    return;

                data_grid_cars.ItemsSource = List_cars;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void DatePeriod_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isClearingPeriod)
                LoadData();
        }

        private void Button_Click_clear_period(object sender, RoutedEventArgs e)
        {
            isClearingPeriod = true;
            dp_date_from.SelectedDate = null;
            dp_date_to.SelectedDate = null;
            isClearingPeriod = false;
            LoadData();
        }
        private string FormatEngine(object engineValue)
        {
            string engineJson = engineValue?.ToString();
            if (string.IsNullOrWhiteSpace(engineJson))
                return "";
            try
            {
                using (var doc = JsonDocument.Parse(engineJson))
                {
                    return string.Join(", ", doc.RootElement.EnumerateObject().Select(p => $"{p.Name}: {p.Value}"));
                }
            }
            catch
            {
                return engineJson;
            }
        }
        private void tb_search_got_focus(object sender, EventArgs e)
        {
            if (tb_search.Text == tb_search_standart_text)
            {
                tb_search.Text = "";
                tb_search.Foreground = Brushes.Black;
            }
        }
        private void tb_search_text_changed(object sender, RoutedEventArgs e)
        {
            Search.Search_text = tb_search.Text;
            if (Search.Search_text != tb_search_standart_text)
            {
                string search_text = tb_search.Text.Trim();
                LoadData();
            }
        }
        private void tb_search_lost_focus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_search.Text))
            {
                tb_search.Text = tb_search_standart_text;
                tb_search.Foreground = Brushes.Gray;
            }
        }
        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            Ordinary_menu ordinary_menu = new Ordinary_menu();
            ordinary_menu.Show();
            this.Close();
        }
    }
}