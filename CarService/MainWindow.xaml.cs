using CarService.Entities;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace CarService
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Бинды для удобного пользования программой
            PreviewKeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };
            PreviewKeyDown += (s, e) => { if (e.Key == Key.F11) Application.Current.MainWindow.WindowState = WindowState.Normal; };
            PreviewKeyDown += (s, e) => { if (e.Key == Key.F12) Application.Current.MainWindow.WindowState = WindowState.Maximized; };

            //Открытие страницы LoginPage при запуске программы
            FrameMain.Navigate(new Pages.LoginPage());

            //Импорт клиентов.
            //var clientData = File.ReadAllLines(@"H:\Учебная Практика\Практика УП 05.01\Ресурсы\Сессия 1\Клиенты.txt");
            //for (int i = 0; i < clientData.Count(); i++)
            //{
            //    var currentClient = clientData[i].Split('\t');
            //    var clientForDB = new Client
            //    {
            //        LastName = currentClient[0],
            //        FirstName = currentClient[1],
            //        Patronymic = currentClient[2],
            //        GenderCode = currentClient[3],
            //        Phone = currentClient[4],
            //        Birthday = DateTime.Parse(currentClient[5]),
            //        Email = currentClient[6],
            //        RegistrationDate = DateTime.Parse(currentClient[7]),
            //    };

            //    App.Context.Clients.Add(clientForDB);
            //    App.Context.SaveChanges();
            //}

            //Импорт услуг.
            //var servicesData = File.ReadAllLines(@"H:\Учебная Практика\Практика УП 05.01\Ресурсы\Сессия 1\Услуги.txt");
            //for (int i = 0; i < servicesData.Count(); i++)
            //{
            //    var currentService = servicesData[i].Split('\t');
            //    var serviceForDB = new Service
            //    {
            //        Title = currentService[0].Trim(),
            //        MainImage = File.ReadAllBytes(@"H:\Учебная Практика\Практика УП 05.01\Ресурсы\Сессия 1\services_a_import\" + currentService[1].Trim()),
            //        DurationInSeconds = Int32.Parse(currentService[2].Trim()),
            //        Cost = Decimal.Parse(currentService[3]),
            //        Discount = Double.Parse(currentService[4]),
            //    };

            //    App.Context.Services.Add(serviceForDB);
            //    App.Context.SaveChanges();
            //}

            //Импорт услуг клиентов
            //var clientServicesData = File.ReadAllLines(@"H:\Учебная Практика\Практика УП 05.01\Ресурсы\Сессия 1\Услуги Клиента.txt");
            //for (int i = 0; i < clientServicesData.Count(); i++)
            //{
            //    var currentClientService = clientServicesData[i].Split('\t');

            //    var clientServiceForDB = new ClientService
            //    {
            //        ServiceID = App.Context.Services.ToList().FirstOrDefault(p => p.Title == currentClientService[0]).ID,
            //        StartTime = DateTime.Parse(currentClientService[1]),
            //        ClientID = App.Context.Clients.ToList().FirstOrDefault(p => p.LastName == currentClientService[2]).ID,
            //    };

            //    App.Context.ClientServices.Add(clientServiceForDB);
            //    App.Context.SaveChanges();
            //}
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            //Обработка функциональности при нажатии на кнопку назад
            if (FrameMain.CanGoBack)
            { 
                FrameMain.GoBack(); 
            }
        }
    }
}
