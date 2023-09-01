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
    /// Логика взаимодействия для ServicesPage.xaml
    /// </summary>
    public partial class ServicesPage : Page
    {
        public ServicesPage()
        {
            InitializeComponent();

            //1 - админ, 2 - пользователь.
            if (App.CurrentUser.RoleId == 1)
            {
                BtnAddService.Visibility = Visibility.Visible;
            }
            else
            {
                BtnAddService.Visibility = Visibility.Collapsed;
            }

            ComboDiscount.SelectedIndex = 0;
            ComboSortBy.SelectedIndex = 0;
            UpdateServices();
        }

        //Переход на страницу добавление услуг
        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditServicePage());
        }

        //Переход на страницу редактирование услуг на основе уже введённых (сохранённых в БД) данных
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var currentService = (sender as Button).DataContext as Entities.Service;
            NavigationService.Navigate(new AddEditServicePage(currentService));
        }

        //Проверка, существования данных в БД и удаление услуги
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var currentService = (sender as Button).DataContext as Entities.Service;

            //Ввод подтверждающего сообщения
            if (MessageBox.Show($"Вы уверены, что хотите удалить услугу: " + $"{currentService.Title}?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                App.Context.Service.Remove(currentService);
                App.Context.SaveChanges();
                UpdateServices();
            }

        }

        //Обновление данных на основе выбранного фильтра
        private void ComboSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateServices();
        }

        //Обновление данных на основе выбранного фильтра
        private void ComboDiscount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateServices();
        }

        //Обновление данных на основе выбранного фильтра
        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateServices();
        }
        private void UpdateServices()
        {
            var services = App.Context.Service.ToList();

            //Сортировка по цене
            if (ComboSortBy.SelectedIndex == 0)
                services = services.OrderBy(p => p.CostWithDiscount).ToList();
            else
                services = services.OrderByDescending(p => p.CostWithDiscount).ToList();

            //Сортировака по размеру скидки
            if (ComboDiscount.SelectedIndex == 1)
                services = services.Where(p => p.Discount >= 0 && p.Discount < 0.05).ToList();
            if (ComboDiscount.SelectedIndex == 2)
                services = services.Where(p => p.Discount >= 0.05 && p.Discount < 0.15).ToList();
            if (ComboDiscount.SelectedIndex == 3)
                services = services.Where(p => p.Discount >= 0.15 && p.Discount < 0.3).ToList();
            if (ComboDiscount.SelectedIndex == 4)
                services = services.Where(p => p.Discount >= 0.3 && p.Discount < 0.7).ToList();
            if (ComboDiscount.SelectedIndex == 5)
                services = services.Where(p => p.Discount >= 0.7 && p.Discount < 1).ToList();

            //Поиск по названию
            services = services.Where(p => p.Title.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            LViewServices.ItemsSource = services;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateServices();
        }
    }
}