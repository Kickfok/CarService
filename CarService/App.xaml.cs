using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CarService
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        //Cтатическое свойство Context, возвращающее новый экземпляр модели Entity Framework.
        public static Entities.СarServiceEntities Context { get; } = new Entities.СarServiceEntities();
        
        //Свойство для хранения авторизированного пользователя
        public static Entities.User CurrentUser = null;
    }
}
