using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UserRegistration;

namespace CarService.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            Loaded += (s, e) => TBoxLogin.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = TBoxLogin.Text.Trim();
            var password = PBoxPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Entities.User currentUser;
            try
            {
                //Поиск пользователя по логину, пароль сверяется с хешем из БД
                currentUser = App.Context.User.FirstOrDefault(p => p.Login == login);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте строку подключения в App.config " +
                                "и что база создана (см. database/README.md).\n\n" + ex.GetBaseException().Message,
                                "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (currentUser != null && PasswordHasher.Verify(password, currentUser.Password))
            {
                //Запись пользователя
                App.CurrentUser = currentUser;
                PBoxPassword.Clear();
                NavigationService.Navigate(new ServicesPage());
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
