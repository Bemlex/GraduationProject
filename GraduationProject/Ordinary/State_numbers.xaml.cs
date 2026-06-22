using GraduationProject.Admin;
using Npgsql;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для State_numbers.xaml
    /// </summary>
    public partial class State_numbers : Window
    {
        Npgsql.NpgsqlConnection con = new NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        Npgsql.NpgsqlCommand cmd = new NpgsqlCommand();
        private List<State_numbersView> list_state_numbers = new List<State_numbersView>();
        public State_numbers()
        {
            InitializeComponent();
            try
            {
                cmd.Connection = con;
            }
            catch (PostgresException ex)
            {
                MessageBox.Show(ex.Message);
            }
            LoadData();
        }
        private async void LoadData()
        {
            try
            {
                await con.OpenAsync();
                string sql = @"
                SELECT 
                    s.number,
                    s.region,

                    pe.last_name AS owner_last_name,
                    pe.first_name AS owner_first_name,
                    pe.middle_name AS owner_middle_name,

                    s.date_reg,

                    p.last_name AS employee_last_name,
                    p.first_name AS employee_first_name,
                    p.middle_name AS employee_middle_name
                FROM State_numbers s 
                LEFT JOIN Employees e ON s.employee_token = e.token
                LEFT JOIN People p ON e.people_id = p.id
                LEFT JOIN Cars c ON c.state_number_id = s.id
                LEFT JOIN People pe ON c.owner_id = pe.id;";
                using (var cmd = new NpgsqlCommand(sql, con))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list_state_numbers.Add(new State_numbersView
                        {
                            Государственный_номер = reader["number"].ToString(),
                            Регион = reader["region"].ToString(),

                            Фамилия_владельца = reader["owner_last_name"].ToString(),
                            Имя_владельца = reader["owner_first_name"].ToString(),
                            Отчество_владельца = reader["owner_middle_name"].ToString(),

                            Дата_регистрации = DateTime.Parse(reader["date_reg"].ToString()).ToString("dd.MM.yyyy"),

                            Фамилия = reader["employee_last_name"].ToString(),
                            Имя = reader["employee_first_name"].ToString(),
                            Отчество = reader["employee_middle_name"].ToString()
                        });
                    }
                }
                data_grid_employees.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[0].Header = new TextBlock { Text = "Государственный номер", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[1].Header = new TextBlock { Text = "Регион", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[2].Header = new TextBlock { Text = "Фамилия владельца", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[3].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[3].Header = new TextBlock { Text = "Имя владельца", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[4].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[4].Header = new TextBlock { Text = "Отчество владельца", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[5].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[5].Header = new TextBlock { Text = "Дата регистрации", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[6].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[6].Header = new TextBlock { Text = "Фамилия выдавшего", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[7].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[7].Header = new TextBlock { Text = "Имя выдавшего", FontWeight = FontWeights.Bold };

                data_grid_employees.Columns[8].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                data_grid_employees.Columns[8].Header = new TextBlock { Text = "Отчество выдавшего", FontWeight = FontWeights.Bold };
                data_grid_employees.ItemsSource = list_state_numbers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
            finally
            {
                await con.CloseAsync();
            }
        }
        private void Image_click(object sender, EventArgs e)
        {
            if (sender is Image img && img.Source != null)
            {
                image_big.Visibility = Visibility.Visible;
                image_big.Source = img.Source;
            }
        }
        private void Big_image_click(object sender, EventArgs e)
        {
            image_big.Visibility = Visibility.Hidden;
        }
        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            Ordinary_menu ordinary_menu = new Ordinary_menu();
            ordinary_menu.Show();
            this.Close();
        }
        private void TextBox_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (list_state_numbers == null)
                return;

            string searchText = TextBox_search.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                data_grid_employees.ItemsSource = list_state_numbers;
                return;
            }

            data_grid_employees.ItemsSource = list_state_numbers.Where(item =>
                (item.Государственный_номер ?? "").ToLower().Contains(searchText) ||
                (item.Регион ?? "").ToLower().Contains(searchText) ||
                (item.Фамилия_владельца ?? "").ToLower().Contains(searchText) ||
                (item.Имя_владельца ?? "").ToLower().Contains(searchText) ||
                (item.Отчество_владельца ?? "").ToLower().Contains(searchText) ||
                (item.Дата_регистрации ?? "").ToLower().Contains(searchText) ||
                (item.Фамилия ?? "").ToLower().Contains(searchText) ||
                (item.Имя ?? "").ToLower().Contains(searchText) ||
                (item.Отчество ?? "").ToLower().Contains(searchText)
            ).ToList();
        }
    }
}