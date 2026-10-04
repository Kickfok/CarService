using System.Windows;

namespace CarService
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Единственный экземпляр контекста Entity Framework на все время работы приложения.
        public static Entities.CarServiceEntities Context { get; } = new Entities.CarServiceEntities();

        // Авторизованный пользователь. Сбрасывается при возврате на страницу авторизации.
        public static Entities.User CurrentUser { get; set; }

        // Признак того, что текущий пользователь - администратор.
        public static bool IsAdmin => CurrentUser != null && CurrentUser.RoleId == Roles.Admin;
    }
}
