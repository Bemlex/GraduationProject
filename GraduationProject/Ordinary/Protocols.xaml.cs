using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

namespace GraduationProject.Ordinary
{
    /// <summary>
    /// Логика взаимодействия для Protocols.xaml
    /// </summary>
    public partial class Protocols : Window
    {
        private const string DbConnectionString = "Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD";
        NpgsqlConnection con = new Npgsql.NpgsqlConnection(DbConnectionString);
        NpgsqlCommand cmd = new NpgsqlCommand();
        private DataTable data_table_protocols;
        private class LawForProtocol
        {
            public int Id { get; set; }
            public string Article { get; set; }
            public string Code { get; set; }
            public string Title { get; set; }
            public string Display => $"{Article} {Code} - {Title}";
        }

        private class EmployeeForProtocol
        {
            public string Token { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string Fio => $"{LastName} {FirstName} {MiddleName}".Trim();
        }

        private class NewProtocolData
        {
            public DateTime Date { get; set; }
            public string Source { get; set; }
            public string Place { get; set; }
            public int LawId { get; set; }
            public string LawArticle { get; set; }
            public string LawCode { get; set; }
            public int PeopleId { get; set; }
            public string PeopleLastName { get; set; }
            public string PeopleFirstName { get; set; }
            public string PeopleMiddleName { get; set; }
            public int CarId { get; set; }
            public string CarMark { get; set; }
            public string CarModel { get; set; }
            public string CarColor { get; set; }
            public int? StateNumberId { get; set; }
            public string StateNumber { get; set; }
            public string StateRegion { get; set; }
            public string MemberRole { get; set; }
            public string Description { get; set; }
            public EmployeeForProtocol Employee { get; set; }
        }
        public Protocols()
        {
            InitializeComponent();
            cmd.Connection = con;
            LoadData();
        }
        private async void LoadData()
        {
            try
            {
                await con.OpenAsync();

                string sql = @"
    SELECT 
        p.id AS protocol_id,
        to_char(p.date, 'HH24:MI DD.MM.YYYY') AS protocol_date,
        p.source AS protocol_source,
        p.place AS place,

        l.article AS law_article,
        l.code AS law_code,

        c.id AS car_id,
        m.mark AS car_mark,
        mm.model AS car_model,
        c.color AS car_color,

        vr.state_number_id AS state_number_id,
        s.number AS state_number,
        s.region AS state_region,

        pm.member_role AS member_role,

        p_people.id AS people_id,
        p_people.last_name AS people_last_name,
        p_people.first_name AS people_first_name,
        p_people.middle_name AS people_middle_name,

        p.description AS description,

        e.token AS employee_token,
        p_employee.last_name AS employee_last_name,
        p_employee.first_name AS employee_first_name,
        p_employee.middle_name AS employee_middle_name,

        p.law_id AS law_id

    FROM Protocols p
    LEFT JOIN Protocol_members pm ON p.id = pm.protocol_id
    LEFT JOIN Laws l ON p.law_id = l.id
    LEFT JOIN Employees e ON p.employee_token = e.token
    LEFT JOIN People p_employee ON e.people_id = p_employee.id
    LEFT JOIN People p_people ON pm.people_id = p_people.id
    LEFT JOIN Cars c ON pm.car_id = c.id
    LEFT JOIN Mark_models mm ON c.mark_model_id = mm.id
    LEFT JOIN Marks m ON mm.mark_id = m.id

    LEFT JOIN LATERAL (
        SELECT vr.state_number_id
        FROM Vehicle_registrations vr
        WHERE vr.car_id = c.id
        ORDER BY 
            CASE WHEN vr.date_end IS NULL THEN 0 ELSE 1 END,
            vr.date_reg DESC,
            vr.id DESC
        LIMIT 1
    ) vr ON true

    LEFT JOIN State_numbers s ON vr.state_number_id = s.id

    ORDER BY p.date DESC;";

                using (var cmd = new NpgsqlCommand(sql, con))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    data_table_protocols = new DataTable();
                    data_table_protocols.Load(reader);
                    data_table_protocols.CaseSensitive = false;
                }

                data_table_protocols.AcceptChanges();

                foreach (DataColumn col in data_table_protocols.Columns)
                {
                    col.ReadOnly = false;
                }

                data_grid_protocols.ItemsSource = data_table_protocols.DefaultView;
                data_grid_protocols.MinRowHeight = 35;
                data_grid_protocols.RowHeight = double.NaN;

                // Скрытые служебные ID
                // data_grid_protocols.Columns[0].Visibility = Visibility.Collapsed;  // protocol_id, если id протокола нужно скрыть
                data_grid_protocols.Columns[6].Visibility = Visibility.Collapsed;   // car_id
                data_grid_protocols.Columns[10].Visibility = Visibility.Collapsed;  // state_number_id
                data_grid_protocols.Columns[14].Visibility = Visibility.Collapsed;  // people_id
                data_grid_protocols.Columns[19].Visibility = Visibility.Collapsed;  // employee_token

                data_grid_protocols.Columns[0].Width = new DataGridLength(90, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[0].Header = new TextBlock { Text = "№ протокола", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[1].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[1].Header = new TextBlock { Text = "Дата протокола", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[2].Width = new DataGridLength(120, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[2].Header = new TextBlock { Text = "Источник", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[3].Width = new DataGridLength(300, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[3].Header = new TextBlock { Text = "Место", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[4].Width = new DataGridLength(100, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[4].Header = new TextBlock { Text = "Статья", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[5].Width = new DataGridLength(100, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[5].Header = new TextBlock { Text = "Код", FontWeight = FontWeights.Bold };

                // Columns[6] car_id скрыт

                data_grid_protocols.Columns[7].Width = new DataGridLength(120, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[7].Header = new TextBlock { Text = "Марка", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[8].Width = new DataGridLength(120, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[8].Header = new TextBlock { Text = "Модель", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[9].Width = new DataGridLength(100, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[9].Header = new TextBlock { Text = "Цвет", FontWeight = FontWeights.Bold };

                // Columns[10] state_number_id скрыт

                data_grid_protocols.Columns[11].Width = new DataGridLength(120, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[11].Header = new TextBlock { Text = "Гос. номер", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[12].Width = new DataGridLength(90, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[12].Header = new TextBlock { Text = "Регион", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[13].Width = new DataGridLength(160, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[13].Header = new TextBlock { Text = "Роль участника", FontWeight = FontWeights.Bold };

                // Columns[14] people_id скрыт

                data_grid_protocols.Columns[15].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[15].Header = new TextBlock { Text = "Фамилия участника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[16].Width = new DataGridLength(160, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[16].Header = new TextBlock { Text = "Имя участника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[17].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[17].Header = new TextBlock { Text = "Отчество участника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[18].Width = new DataGridLength(350, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[18].Header = new TextBlock { Text = "Описание", FontWeight = FontWeights.Bold };

                // Columns[19] employee_token скрыт

                data_grid_protocols.Columns[20].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[20].Header = new TextBlock { Text = "Фамилия сотрудника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[21].Width = new DataGridLength(160, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[21].Header = new TextBlock { Text = "Имя сотрудника", FontWeight = FontWeights.Bold };

                data_grid_protocols.Columns[22].Width = new DataGridLength(180, DataGridLengthUnitType.Pixel);
                data_grid_protocols.Columns[22].Header = new TextBlock { Text = "Отчество сотрудника", FontWeight = FontWeights.Bold };

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            finally
            {
                await con.CloseAsync();
            }
        }
        private void Button_Click_edit(object sender, RoutedEventArgs e)
        {
        }
        private async void Button_Click_add(object sender, RoutedEventArgs e)
        {
            if (data_table_protocols == null)
            {
                MessageBox.Show("Данные ещё не загружены.");
                return;
            }

            try
            {
                TextBox_search.Text = "";

                List<string> sources = await LoadEnumValuesAsync("protocol_source_enum");
                List<string> roles = await LoadEnumValuesAsync("protocol_role_member");
                List<LawForProtocol> laws = await LoadLawsAsync();
                DataTable people = await LoadPeopleForSelectAsync();
                DataTable cars = await LoadCarsForSelectAsync();
                EmployeeForProtocol employee = await LoadCurrentEmployeeAsync();

                if (employee == null || string.IsNullOrWhiteSpace(employee.Token))
                {
                    MessageBox.Show("К текущему пользователю не привязан сотрудник. Невозможно автоматически заполнить ФИО сотрудника.");
                    return;
                }

                ProtocolAddWindow addWindow = new ProtocolAddWindow(
                    sources,
                    roles,
                    laws,
                    people,
                    cars,
                    employee)
                {
                    Owner = this
                };

                if (addWindow.ShowDialog() != true)
                    return;

                AddProtocolRow(addWindow.ProtocolData);

                Button_save.IsEnabled = true;

                DataRowView rowView = data_table_protocols.DefaultView
                    .Cast<DataRowView>()
                    .LastOrDefault(row => row.Row.RowState == DataRowState.Added);

                if (rowView != null)
                {
                    data_grid_protocols.SelectedItem = rowView;
                    data_grid_protocols.ScrollIntoView(rowView);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении протокола: {ex.Message}");
            }
        }
        private async void Button_Click_delete(object sender, RoutedEventArgs e)
        {
        }
        private async void Button_Click_save(object sender, RoutedEventArgs e)
        {
            if (data_table_protocols == null)
                return;

            DataRow[] newRows = data_table_protocols.Select(null, null, DataViewRowState.Added);

            if (newRows.Length == 0)
            {
                MessageBox.Show("Нет новых протоколов для сохранения.");
                return;
            }

            try
            {
                await con.OpenAsync();

                using (var transaction = con.BeginTransaction())
                {
                    foreach (DataRow row in newRows)
                    {
                        ValidateProtocolRow(row);

                        int protocolId;

                        string sqlInsertProtocol = @"
                            INSERT INTO Protocols
                                (law_id, date, place, description, source, employee_token)
                            VALUES
                                (@law_id, @date, @place, @description, CAST(@source AS protocol_source_enum), @employee_token)
                            RETURNING id;";

                        using (var cmdInsertProtocol = new NpgsqlCommand(sqlInsertProtocol, con, transaction))
                        {
                            cmdInsertProtocol.Parameters.AddWithValue("law_id", Convert.ToInt32(row["law_id"]));
                            cmdInsertProtocol.Parameters.AddWithValue("date", ParseProtocolDate(row["protocol_date"]?.ToString()));
                            cmdInsertProtocol.Parameters.AddWithValue("place", row["place"]?.ToString() ?? "");
                            cmdInsertProtocol.Parameters.AddWithValue("description", row["description"]?.ToString() ?? "");
                            cmdInsertProtocol.Parameters.AddWithValue("source", row["protocol_source"]?.ToString() ?? "");
                            cmdInsertProtocol.Parameters.AddWithValue("employee_token", row["employee_token"]?.ToString() ?? "");

                            protocolId = Convert.ToInt32(await cmdInsertProtocol.ExecuteScalarAsync());
                        }

                        string sqlInsertMember = @"
                            INSERT INTO Protocol_members
                                (protocol_id, people_id, car_id, member_role)
                            VALUES
                                (@protocol_id, @people_id, @car_id, CAST(@member_role AS protocol_role_member));";

                        using (var cmdInsertMember = new NpgsqlCommand(sqlInsertMember, con, transaction))
                        {
                            cmdInsertMember.Parameters.AddWithValue("protocol_id", protocolId);
                            cmdInsertMember.Parameters.AddWithValue("people_id", Convert.ToInt32(row["people_id"]));
                            cmdInsertMember.Parameters.AddWithValue("car_id", Convert.ToInt32(row["car_id"]));
                            cmdInsertMember.Parameters.AddWithValue("member_role", row["member_role"]?.ToString() ?? "");

                            await cmdInsertMember.ExecuteNonQueryAsync();
                        }

                        row["protocol_id"] = protocolId;
                    }

                    transaction.Commit();
                }

                MessageBox.Show("Протокол сохранён.");

                Button_save.IsEnabled = false;
                data_table_protocols.AcceptChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
            }
        }
        private void AddProtocolRow(NewProtocolData protocol)
        {
            DataRow row = data_table_protocols.NewRow();

            row["protocol_id"] = DBNull.Value;
            row["protocol_date"] = protocol.Date.ToString("HH:mm dd.MM.yyyy");
            row["protocol_source"] = protocol.Source;
            row["place"] = protocol.Place;
            row["law_article"] = protocol.LawArticle;
            row["law_code"] = protocol.LawCode;
            row["car_id"] = protocol.CarId;
            row["car_mark"] = protocol.CarMark;
            row["car_model"] = protocol.CarModel;
            row["car_color"] = protocol.CarColor;
            row["state_number_id"] = protocol.StateNumberId.HasValue
                ? (object)protocol.StateNumberId.Value
                : DBNull.Value;
            row["state_number"] = protocol.StateNumber;
            row["state_region"] = int.TryParse(protocol.StateRegion, out int region)
                ? (object)region
                : DBNull.Value;
            row["member_role"] = protocol.MemberRole;
            row["people_id"] = protocol.PeopleId;
            row["people_last_name"] = protocol.PeopleLastName;
            row["people_first_name"] = protocol.PeopleFirstName;
            row["people_middle_name"] = protocol.PeopleMiddleName;
            row["description"] = protocol.Description;
            row["employee_token"] = protocol.Employee.Token;
            row["employee_last_name"] = protocol.Employee.LastName;
            row["employee_first_name"] = protocol.Employee.FirstName;
            row["employee_middle_name"] = protocol.Employee.MiddleName;
            row["law_id"] = protocol.LawId;

            data_table_protocols.Rows.Add(row);
        }

        private async Task<List<string>> LoadEnumValuesAsync(string enumTypeName)
        {
            List<string> values = new List<string>();

            string sql = @"
                SELECT e.enumlabel
                FROM pg_type t
                JOIN pg_enum e ON t.oid = e.enumtypid
                WHERE t.typname = @type_name
                ORDER BY e.enumsortorder;";

            using (var connection = new NpgsqlConnection(DbConnectionString))
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("type_name", enumTypeName);

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        values.Add(reader["enumlabel"].ToString());
                    }
                }
            }

            return values;
        }

        private async Task<List<LawForProtocol>> LoadLawsAsync()
        {
            List<LawForProtocol> laws = new List<LawForProtocol>();

            string sql = @"
                SELECT id, article, code, title
                FROM Laws
                ORDER BY article, code, title;";

            using (var connection = new NpgsqlConnection(DbConnectionString))
            using (var command = new NpgsqlCommand(sql, connection))
            {
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        laws.Add(new LawForProtocol
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Article = reader["article"]?.ToString() ?? "",
                            Code = reader["code"]?.ToString() ?? "",
                            Title = reader["title"]?.ToString() ?? ""
                        });
                    }
                }
            }

            return laws;
        }

        private async Task<DataTable> LoadPeopleForSelectAsync()
        {
            string sql = @"
                SELECT
                    id AS people_id,
                    last_name AS ""Фамилия"",
                    first_name AS ""Имя"",
                    middle_name AS ""Отчество"",
                    to_char(birthday, 'DD.MM.YYYY') AS ""Дата рождения"",
                    passport AS ""Паспорт"",
                    phone AS ""Телефон""
                FROM People
                ORDER BY last_name, first_name, middle_name;";

            return await LoadDataTableAsync(sql);
        }

        private async Task<DataTable> LoadCarsForSelectAsync()
        {
            string sql = @"
                SELECT
                    c.id AS car_id,
                    vr.state_number_id,
                    m.mark AS ""Марка"",
                    mm.model AS ""Модель"",
                    c.color AS ""Цвет"",
                    s.number AS ""Гос. номер"",
                    s.region AS ""Регион"",
                    concat_ws(' ', owner.last_name, owner.first_name, owner.middle_name) AS ""Владелец""
                FROM Cars c
                LEFT JOIN Mark_models mm ON c.mark_model_id = mm.id
                LEFT JOIN Marks m ON mm.mark_id = m.id
                LEFT JOIN LATERAL (
                    SELECT vr.people_id, vr.state_number_id
                    FROM Vehicle_registrations vr
                    WHERE vr.car_id = c.id
                    ORDER BY
                        CASE WHEN vr.date_end IS NULL THEN 0 ELSE 1 END,
                        vr.date_reg DESC,
                        vr.id DESC
                    LIMIT 1
                ) vr ON true
                LEFT JOIN State_numbers s ON vr.state_number_id = s.id
                LEFT JOIN People owner ON vr.people_id = owner.id
                ORDER BY m.mark, mm.model, s.number;";

            return await LoadDataTableAsync(sql);
        }

        private async Task<DataTable> LoadDataTableAsync(string sql)
        {
            using (var connection = new NpgsqlConnection(DbConnectionString))
            using (var command = new NpgsqlCommand(sql, connection))
            {
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    table.CaseSensitive = false;
                    return table;
                }
            }
        }

        private async Task<EmployeeForProtocol> LoadCurrentEmployeeAsync()
        {
            if (string.IsNullOrWhiteSpace(GraduationProject.Session.Login))
                return null;

            string sql = @"
                SELECT
                    u.employee_token,
                    p.last_name,
                    p.first_name,
                    p.middle_name
                FROM Users u
                LEFT JOIN Employees e ON u.employee_token = e.token
                LEFT JOIN People p ON e.people_id = p.id
                WHERE u.login = @login;";

            using (var connection = new NpgsqlConnection(DbConnectionString))
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("login", GraduationProject.Session.Login);

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return null;

                    return new EmployeeForProtocol
                    {
                        Token = reader["employee_token"] == DBNull.Value ? "" : reader["employee_token"].ToString(),
                        LastName = reader["last_name"] == DBNull.Value ? "" : reader["last_name"].ToString(),
                        FirstName = reader["first_name"] == DBNull.Value ? "" : reader["first_name"].ToString(),
                        MiddleName = reader["middle_name"] == DBNull.Value ? "" : reader["middle_name"].ToString()
                    };
                }
            }
        }

        private void ValidateProtocolRow(DataRow row)
        {
            if (row.IsNull("law_id"))
                throw new Exception("Выберите статью закона.");

            if (string.IsNullOrWhiteSpace(row["protocol_source"]?.ToString()))
                throw new Exception("Выберите источник.");

            if (string.IsNullOrWhiteSpace(row["place"]?.ToString()))
                throw new Exception("Заполните место.");

            if (row.IsNull("people_id"))
                throw new Exception("Выберите участника.");

            if (row.IsNull("car_id"))
                throw new Exception("Выберите автомобиль.");

            if (string.IsNullOrWhiteSpace(row["member_role"]?.ToString()))
                throw new Exception("Выберите роль участника.");

            if (string.IsNullOrWhiteSpace(row["employee_token"]?.ToString()))
                throw new Exception("Не удалось определить сотрудника.");
        }

        private DateTime ParseProtocolDate(string value)
        {
            if (DateTime.TryParseExact(
                value,
                "HH:mm dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date))
            {
                return date;
            }

            throw new Exception("Дата протокола должна быть в формате ЧЧ:ММ ДД.ММ.ГГГГ.");
        }

        private void TextBox_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (data_table_protocols == null)
                return;

            string searchText = TextBox_search.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                data_table_protocols.DefaultView.RowFilter = "";
                return;
            }

            searchText = searchText.Replace("'", "''");

            data_table_protocols.DefaultView.RowFilter = $@"
        Convert(protocol_id, 'System.String') LIKE '%{searchText}%'
        OR protocol_date LIKE '%{searchText}%'
        OR protocol_source LIKE '%{searchText}%'
        OR place LIKE '%{searchText}%'
        OR law_article LIKE '%{searchText}%'
        OR law_code LIKE '%{searchText}%'
        OR car_mark LIKE '%{searchText}%'
        OR car_model LIKE '%{searchText}%'
        OR car_color LIKE '%{searchText}%'
        OR state_number LIKE '%{searchText}%'
        OR member_role LIKE '%{searchText}%'
        OR people_last_name LIKE '%{searchText}%'
        OR people_first_name LIKE '%{searchText}%'
        OR people_middle_name LIKE '%{searchText}%'
        OR description LIKE '%{searchText}%'
        OR employee_last_name LIKE '%{searchText}%'
        OR employee_first_name LIKE '%{searchText}%'
        OR employee_middle_name LIKE '%{searchText}%'
    ";
        }
        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            Ordinary_menu ordinary_menu = new Ordinary_menu();
            ordinary_menu.Show();
            this.Close();
        }

        private class ProtocolAddWindow : Window
        {
            private readonly List<string> sources;
            private readonly List<string> roles;
            private readonly List<LawForProtocol> laws;
            private readonly DataTable peopleTable;
            private readonly DataTable carsTable;
            private readonly EmployeeForProtocol employee;

            private DatePicker datePicker;
            private TextBox timeTextBox;
            private ComboBox sourceComboBox;
            private ComboBox lawComboBox;
            private TextBox placeTextBox;
            private TextBox personTextBox;
            private TextBox carTextBox;
            private ComboBox roleComboBox;
            private TextBox descriptionTextBox;
            private TextBox employeeTextBox;

            private DataRowView selectedPerson;
            private DataRowView selectedCar;

            public NewProtocolData ProtocolData { get; private set; }

            public ProtocolAddWindow(
                List<string> sources,
                List<string> roles,
                List<LawForProtocol> laws,
                DataTable peopleTable,
                DataTable carsTable,
                EmployeeForProtocol employee)
            {
                this.sources = sources;
                this.roles = roles;
                this.laws = laws;
                this.peopleTable = peopleTable;
                this.carsTable = carsTable;
                this.employee = employee;

                Title = "Добавление протокола";
                Width = 850;
                Height = 720;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;

                BuildInterface();
            }

            private void BuildInterface()
            {
                Grid root = new Grid();
                root.Margin = new Thickness(18);
                root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                ScrollViewer scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };

                StackPanel form = new StackPanel();

                datePicker = new DatePicker
                {
                    SelectedDate = DateTime.Now.Date,
                    FontSize = 18,
                    Height = 36
                };
                AddControl(form, "Дата протокола", datePicker);

                timeTextBox = AddTextBox(form, "Время протокола (ЧЧ:ММ)");
                timeTextBox.Text = DateTime.Now.ToString("HH:mm");

                sourceComboBox = AddComboBox(form, "Источник", sources);

                lawComboBox = new ComboBox
                {
                    ItemsSource = laws,
                    DisplayMemberPath = "Display",
                    SelectedIndex = laws.Count > 0 ? 0 : -1,
                    IsTextSearchEnabled = true,
                    FontSize = 18,
                    Height = 36
                };
                AddControl(form, "Статья закона", lawComboBox);

                placeTextBox = AddTextBox(form, "Место");

                personTextBox = AddSelector(form, "Участник", Button_Click_select_person);
                carTextBox = AddSelector(form, "Автомобиль", Button_Click_select_car);

                roleComboBox = AddComboBox(form, "Роль участника", roles);

                descriptionTextBox = AddTextBox(form, "Описание");
                descriptionTextBox.Height = 95;
                descriptionTextBox.AcceptsReturn = true;
                descriptionTextBox.TextWrapping = TextWrapping.Wrap;
                descriptionTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                employeeTextBox = AddTextBox(form, "Сотрудник");
                employeeTextBox.Text = employee.Fio;
                employeeTextBox.IsReadOnly = true;

                scrollViewer.Content = form;
                root.Children.Add(scrollViewer);

                StackPanel buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 16, 0, 0)
                };

                Button cancelButton = new Button
                {
                    Content = "Отмена",
                    Width = 130,
                    Height = 42,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 12, 0)
                };
                cancelButton.Click += (sender, args) => DialogResult = false;

                Button addButton = new Button
                {
                    Content = "Добавить",
                    Width = 140,
                    Height = 42,
                    FontSize = 18
                };
                addButton.Click += Button_Click_add_protocol;

                buttons.Children.Add(cancelButton);
                buttons.Children.Add(addButton);
                Grid.SetRow(buttons, 1);
                root.Children.Add(buttons);

                Content = root;
            }

            private TextBox AddTextBox(StackPanel form, string label)
            {
                TextBox textBox = new TextBox
                {
                    FontSize = 18,
                    Height = 36,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                AddControl(form, label, textBox);

                return textBox;
            }

            private ComboBox AddComboBox(StackPanel form, string label, List<string> values)
            {
                ComboBox comboBox = new ComboBox
                {
                    ItemsSource = values,
                    SelectedIndex = values.Count > 0 ? 0 : -1,
                    FontSize = 18,
                    Height = 36,
                    IsTextSearchEnabled = true
                };

                AddControl(form, label, comboBox);

                return comboBox;
            }

            private TextBox AddSelector(StackPanel form, string label, RoutedEventHandler clickHandler)
            {
                TextBox textBox = new TextBox
                {
                    FontSize = 18,
                    Height = 36,
                    IsReadOnly = true,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                Button button = new Button
                {
                    Content = "Выбрать",
                    Width = 120,
                    FontSize = 16,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                button.Click += clickHandler;

                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.Children.Add(textBox);
                Grid.SetColumn(button, 1);
                grid.Children.Add(button);

                AddControl(form, label, grid);

                return textBox;
            }

            private void AddControl(StackPanel form, string label, FrameworkElement control)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = label,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 8, 0, 4)
                };

                control.Margin = new Thickness(0, 0, 0, 4);

                form.Children.Add(textBlock);
                form.Children.Add(control);
            }

            private void Button_Click_select_person(object sender, RoutedEventArgs e)
            {
                SelectionWindow window = new SelectionWindow("Выбор участника", peopleTable)
                {
                    Owner = this
                };

                if (window.ShowDialog() != true)
                    return;

                selectedPerson = window.SelectedRow;

                personTextBox.Text = string.Join(" ", new[]
                {
                    GetString(selectedPerson, "Фамилия"),
                    GetString(selectedPerson, "Имя"),
                    GetString(selectedPerson, "Отчество")
                }).Trim();
            }

            private void Button_Click_select_car(object sender, RoutedEventArgs e)
            {
                SelectionWindow window = new SelectionWindow("Выбор автомобиля", carsTable)
                {
                    Owner = this
                };

                if (window.ShowDialog() != true)
                    return;

                selectedCar = window.SelectedRow;

                string mark = GetString(selectedCar, "Марка");
                string model = GetString(selectedCar, "Модель");
                string color = GetString(selectedCar, "Цвет");
                string stateNumber = GetString(selectedCar, "Гос. номер");

                carTextBox.Text = $"{mark} {model} {color} {stateNumber}".Trim();
            }

            private void Button_Click_add_protocol(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (datePicker.SelectedDate == null)
                    {
                        MessageBox.Show("Выберите дату протокола.");
                        return;
                    }

                    if (!TimeSpan.TryParseExact(timeTextBox.Text.Trim(), "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan time))
                    {
                        MessageBox.Show("Время должно быть в формате ЧЧ:ММ.");
                        return;
                    }

                    LawForProtocol law = lawComboBox.SelectedItem as LawForProtocol;

                    if (sourceComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Выберите источник.");
                        return;
                    }

                    if (law == null)
                    {
                        MessageBox.Show("Выберите статью закона.");
                        return;
                    }

                    if (roleComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Выберите роль участника.");
                        return;
                    }

                    if (selectedPerson == null)
                    {
                        MessageBox.Show("Выберите участника.");
                        return;
                    }

                    if (selectedCar == null)
                    {
                        MessageBox.Show("Выберите автомобиль.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(placeTextBox.Text))
                    {
                        MessageBox.Show("Заполните место.");
                        return;
                    }

                    DateTime protocolDate = datePicker.SelectedDate.Value.Date.Add(time);

                    ProtocolData = new NewProtocolData
                    {
                        Date = protocolDate,
                        Source = sourceComboBox.SelectedItem?.ToString(),
                        Place = placeTextBox.Text.Trim(),
                        LawId = law.Id,
                        LawArticle = law.Article,
                        LawCode = law.Code,
                        PeopleId = Convert.ToInt32(selectedPerson["people_id"]),
                        PeopleLastName = GetString(selectedPerson, "Фамилия"),
                        PeopleFirstName = GetString(selectedPerson, "Имя"),
                        PeopleMiddleName = GetString(selectedPerson, "Отчество"),
                        CarId = Convert.ToInt32(selectedCar["car_id"]),
                        CarMark = GetString(selectedCar, "Марка"),
                        CarModel = GetString(selectedCar, "Модель"),
                        CarColor = GetString(selectedCar, "Цвет"),
                        StateNumberId = GetNullableInt(selectedCar, "state_number_id"),
                        StateNumber = GetString(selectedCar, "Гос. номер"),
                        StateRegion = GetString(selectedCar, "Регион"),
                        MemberRole = roleComboBox.SelectedItem?.ToString(),
                        Description = descriptionTextBox.Text.Trim(),
                        Employee = employee
                    };

                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при заполнении формы: {ex.Message}");
                }
            }

            private string GetString(DataRowView row, string column)
            {
                return row[column] == DBNull.Value ? "" : row[column].ToString();
            }

            private int? GetNullableInt(DataRowView row, string column)
            {
                if (row[column] == DBNull.Value || string.IsNullOrWhiteSpace(row[column]?.ToString()))
                    return null;

                return Convert.ToInt32(row[column]);
            }
        }

        private class SelectionWindow : Window
        {
            private readonly DataTable table;
            private TextBox searchTextBox;
            private DataGrid dataGrid;

            public DataRowView SelectedRow { get; private set; }

            public SelectionWindow(string title, DataTable table)
            {
                this.table = table;
                this.table.DefaultView.RowFilter = "";

                Title = title;
                Width = 1000;
                Height = 600;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;

                BuildInterface();
            }

            private void BuildInterface()
            {
                Grid root = new Grid();
                root.Margin = new Thickness(12);
                root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                searchTextBox = new TextBox
                {
                    FontSize = 18,
                    Height = 38,
                    Margin = new Thickness(0, 0, 0, 10),
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                searchTextBox.TextChanged += SearchTextBox_TextChanged;
                root.Children.Add(searchTextBox);

                dataGrid = new DataGrid
                {
                    ItemsSource = table.DefaultView,
                    AutoGenerateColumns = true,
                    IsReadOnly = true,
                    SelectionMode = DataGridSelectionMode.Single,
                    SelectionUnit = DataGridSelectionUnit.FullRow,
                    CanUserAddRows = false,
                    FontSize = 16
                };
                dataGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
                dataGrid.MouseDoubleClick += DataGrid_MouseDoubleClick;
                Grid.SetRow(dataGrid, 1);
                root.Children.Add(dataGrid);

                StackPanel buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 12, 0, 0)
                };

                Button cancelButton = new Button
                {
                    Content = "Отмена",
                    Width = 120,
                    Height = 40,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                cancelButton.Click += (sender, args) => DialogResult = false;

                Button selectButton = new Button
                {
                    Content = "Выбрать",
                    Width = 130,
                    Height = 40,
                    FontSize = 16
                };
                selectButton.Click += Button_Click_select;

                buttons.Children.Add(cancelButton);
                buttons.Children.Add(selectButton);
                Grid.SetRow(buttons, 2);
                root.Children.Add(buttons);

                Content = root;
            }

            private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
            {
                string propertyName = e.PropertyName?.ToLower() ?? "";

                if (propertyName.EndsWith("_id") || propertyName == "id")
                {
                    e.Column.Visibility = Visibility.Collapsed;
                }
            }

            private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
            {
                string searchText = searchTextBox.Text.Trim().Replace("'", "''");

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    table.DefaultView.RowFilter = "";
                    return;
                }

                List<string> filters = new List<string>();

                foreach (DataColumn column in table.Columns)
                {
                    string columnName = column.ColumnName.Replace("]", "]]");
                    filters.Add($"Convert([{columnName}], 'System.String') LIKE '%{searchText}%'");
                }

                table.DefaultView.RowFilter = string.Join(" OR ", filters);
            }

            private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            {
                SelectCurrentRow();
            }

            private void Button_Click_select(object sender, RoutedEventArgs e)
            {
                SelectCurrentRow();
            }

            private void SelectCurrentRow()
            {
                if (dataGrid.SelectedItem is DataRowView row)
                {
                    SelectedRow = row;
                    DialogResult = true;
                    return;
                }

                MessageBox.Show("Выберите строку.");
            }
        }
    }
}
