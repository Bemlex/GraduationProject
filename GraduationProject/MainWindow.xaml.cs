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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraduationProject.Ordinary;
using Npgsql;

namespace GraduationProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NpgsqlConnection con = new Npgsql.NpgsqlConnection("Server = localhost; Port = 5432; Username = postgres; Password = 1234; Database = GIBDD");
        NpgsqlCommand cmd = new NpgsqlCommand();
        private Tokens tokens;
        public MainWindow()
        {
            InitializeComponent();
            tokens = new Tokens();
            Loaded += Launch_check_session;
            try
            {
                cmd.Connection = con;
            }
            catch (Npgsql.NpgsqlException ex)
            {
                MessageBox.Show("Ошибка подключения PostgreSQL" + ex);
            }
            tb_login.TextChanged += (s, e) => UpdateAuthButton(); //=> - лямбда-оператор
            tb_password.PasswordChanged += (s, e) => UpdateAuthButton(); //(s, e) - список параметров лямбда-выражения (sender, EventArgs)
        }
        public void UpdateAuthButton ()
        {
            if (!string.IsNullOrWhiteSpace(tb_login.Text) && !string.IsNullOrWhiteSpace(tb_password.Password))
            {
                Button_auth.IsEnabled = true;
            }
            if (string.IsNullOrWhiteSpace(tb_login.Text) || string.IsNullOrWhiteSpace(tb_password.Password))
            {
                Button_auth.IsEnabled = false;
            }
        }
        private async void Launch_check_session (object sender, RoutedEventArgs e)
        {
            await tokens.CheckSession();
        }
        private async Task<string> GetUserFullName(int user_id)
        {
            string sql = @"SELECT concat_ws(' ', p.last_name, p.first_name, p.middle_name) AS full_name
FROM Users u
LEFT JOIN Employees e ON u.employee_token = e.token
LEFT JOIN People p ON e.people_id = p.id
WHERE u.id = @user_id";

            using (var fullNameCmd = new NpgsqlCommand(sql, con))
            {
                fullNameCmd.Parameters.AddWithValue("user_id", user_id);
                object result = await fullNameCmd.ExecuteScalarAsync();
                string fullName = result?.ToString();

                if (string.IsNullOrWhiteSpace(fullName))
                    return Session.Login;

                return fullName;
            }
        }
        public async void Button_Click(object sender, RoutedEventArgs e)
        {
            string login = tb_login.Text;
            string password = tb_password.Password;

            if (string.IsNullOrEmpty(login) && string.IsNullOrEmpty(password) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Каких-то вводимых данных не хватает");
            }
            else
            {
                try
                {
                    await con.OpenAsync();
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("login", login);
                    cmd.Parameters.AddWithValue("password", password);
                    cmd.CommandText = "SELECT * FROM Function_authorization(@login, @password);";
                    bool IsAuthenticated = false;
                    int role_id = 0;
                    int user_id = 0;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            IsAuthenticated = reader.GetBoolean(0);
                            role_id = reader.GetInt32(1);
                            user_id = reader.GetInt32(2);
                        }
                    }

                    if (IsAuthenticated)
                    {
                        Tokens tokens = new Tokens();

                        Session.Login = login;
                        Session.FullName = await GetUserFullName(user_id);

                        tokens.CreateFile(user_id);
                        Entry_roles(role_id);
                    }
                    else
                    {
                        MessageBox.Show("Неверные данные");
                    }
                }
                catch (Npgsql.PostgresException ex)
                {
                    MessageBox.Show("Ошибка: " + ex);
                }
                finally
                {
                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        await con.CloseAsync();
                    }
                }
            }
        }
        public void Entry_roles(int role_id)
        {
            switch (role_id)
            {
                case 1:
                    Admin_menu admin_menu = new Admin_menu();
                    admin_menu.Show();
                    this.Close();
                    break;
                case 3:
                    Ordinary_menu ordinary_menu = new Ordinary_menu();
                    ordinary_menu.Show();
                    this.Close();
                    break;
            }
        }
    }
}