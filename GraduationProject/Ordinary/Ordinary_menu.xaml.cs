using System;
using System.IO;
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
    /// Логика взаимодействия для Ordinary_menu.xaml
    /// </summary>
    public partial class Ordinary_menu : Window
    {
        public Ordinary_menu()
        {
            InitializeComponent();
            tb_info.Text = "Рядовой служащий: " + Session.FullName;
        }
        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(appDirectory, "token.txt");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            MainWindow main_window = new MainWindow();
            main_window.Show();
            this.Close();
        }
        private void Button_Click_mark_models(object sender, RoutedEventArgs e)
        {
            Mark_models mark_models = new Mark_models();
            mark_models.Show();
            this.Close();
        }
        private void Button_Click_people(object sender, RoutedEventArgs e)
        {
            People people = new People();
            people.Show();
            this.Close();
        }
        private void Button_Click_state_numbers(object sender, RoutedEventArgs e)
        {
            State_numbers state_numbers = new State_numbers();
            state_numbers.Show();
            this.Close();
        }
        private void Button_Click_driver_certificates(object sender, RoutedEventArgs e)
        {
            Driver_certificates driver_certificates = new Driver_certificates();
            driver_certificates.Show();
            this.Close();
        }
        private void Button_Click_cars(object sender, RoutedEventArgs e)
        {
            Cars cars = new Cars();
            cars.Show();
            this.Close();
        }
        private void Button_Click_laws(object sender, RoutedEventArgs e)
        {
            Laws laws = new Laws();
            laws.Show();
            this.Close();
        }
        private void Button_Click_procedural_actions(object sender, RoutedEventArgs e)
        {
            ProceduralActions proceduralActions = new ProceduralActions();
            proceduralActions.Show();
            this.Close();
        }
        private void Button_Click_vehicle_defects(object sender, RoutedEventArgs e)
        {
            Vehicle_defects vehicle_defects = new Vehicle_defects();
            vehicle_defects.Show();
            this.Close();
        }

        private void Button_Click_protocols(object sender, RoutedEventArgs e)
        {
            Protocols protocols = new Protocols();
            protocols.Show();
            this.Close();
        }

        private void Button_Click_patrols(object sender, RoutedEventArgs e)
        {
            Patrols patrols = new Patrols();
            patrols.Show();
            this.Close();
        }
    }
}
