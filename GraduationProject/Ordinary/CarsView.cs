using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GraduationProject.Ordinary
{
    public class CarsView
    {
        public string Номер_регистрации { get; set; }
        public string ID_автомобиля { get; set; }

        public string Марка { get; set; }
        public string Модель { get; set; }
        public string Цвет { get; set; }

        public string ID_госномера { get; set; }
        public string Государственный_номер { get; set; }
        public string Регион { get; set; }

        public string Двигатель { get; set; }
        public string Год_выпуска { get; set; }
        public string VIN { get; set; }
        public string Страховая_компания { get; set; }

        public string Фамилия_владельца { get; set; }
        public string Имя_владельца { get; set; }
        public string Отчество_владельца { get; set; }

        public string Дата_регистрации { get; set; }
        public string Дата_окончания { get; set; }

        public string Token_сотрудника { get; set; }
        public string Фамилия_сотрудника { get; set; }
        public string Имя_сотрудника { get; set; }
        public string Отчество_сотрудника { get; set; }
    }
}
