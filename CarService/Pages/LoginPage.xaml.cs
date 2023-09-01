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
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            //Считывания логина и пароля через базу данных
            var currentUser = App.Context.User.FirstOrDefault(p => p.Login == TBoxLogin.Text && p.Password == PBoxPassword.Password);

            if (currentUser != null)
            {
                //Запись пользователя
                App.CurrentUser = currentUser;
                NavigationService.Navigate(new ServicesPage());
            }
            else
                MessageBox.Show("Пользователь с такими данными не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

        }
    }
}
