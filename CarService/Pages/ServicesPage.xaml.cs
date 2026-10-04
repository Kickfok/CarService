using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarService.Pages
{
    /// <summary>
    /// Логика взаимодействия для ServicesPage.xaml
    /// </summary>
    public partial class ServicesPage : Page
    {
        // Диапазоны скидки в процентах для фильтра, по порядку пунктов ComboDiscount (пункт 0 - "Все").
        // Нижняя граница включается, верхняя нет. Сравниваются округленные проценты, а не доли,
        // чтобы погрешность float (0.15 хранится как 0.1499...) не переносила услугу в соседний диапазон.
        private static readonly Tuple<double, double>[] DiscountRanges =
        {
            null,
            Tuple.Create(0.0, 5.0),
            Tuple.Create(5.0, 15.0),
            Tuple.Create(15.0, 30.0),
            Tuple.Create(30.0, 70.0),
            Tuple.Create(70.0, 100.1)
        };

        public ServicesPage()
        {
            InitializeComponent();

            //Добавлять услуги может только администратор
            BtnAddService.Visibility = App.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

            ComboDiscount.SelectedIndex = 0;
            ComboSortBy.SelectedIndex = 0;
        }

        //Переход на страницу добавления услуги
        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditServicePage());
        }

        //Переход на страницу редактирования выбранной услуги
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var currentService = (sender as Button).DataContext as Entities.Service;
            NavigationService.Navigate(new AddEditServicePage(currentService));
        }

        //Удаление услуги с подтверждением
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var currentService = (sender as Button).DataContext as Entities.Service;

            //На услугу уже записаны клиенты: удаление нарушит связи в БД
            if (currentService.ClientService.Any())
            {
                MessageBox.Show($"Услугу \"{currentService.Title}\" нельзя удалить: на нее есть записи клиентов " +
                                $"({currentService.ClientService.Count}).",
                                "Удаление невозможно", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Вы уверены, что хотите удалить услугу \"{currentService.Title}\"?", "Внимание",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                //Дополнительные фотографии услуги удаляются вместе с ней
                App.Context.ServicePhoto.RemoveRange(currentService.ServicePhoto.ToList());
                App.Context.Service.Remove(currentService);
                App.Context.SaveChanges();
            }
            catch (Exception ex)
            {
                //Откат несохраненных изменений, чтобы контекст остался в согласованном состоянии
                foreach (var entry in App.Context.ChangeTracker.Entries().ToList())
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }
                MessageBox.Show("Не удалось удалить услугу.\n\n" + ex.GetBaseException().Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateServices();
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

        //Обновление данных на основе строки поиска
        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateServices();
        }

        private void UpdateServices()
        {
            //Обработчики фильтров срабатывают уже в конструкторе; первую загрузку выполняет Page_Loaded
            if (!IsLoaded)
            {
                return;
            }

            var allServices = App.Context.Service.ToList();
            var services = allServices.AsEnumerable();

            //Фильтр по размеру скидки, услуга без скидки попадает в диапазон от 0 %
            var range = ComboDiscount.SelectedIndex > 0 ? DiscountRanges[ComboDiscount.SelectedIndex] : null;
            if (range != null)
            {
                services = services.Where(p => p.DiscountPercent >= range.Item1 && p.DiscountPercent < range.Item2);
            }

            //Поиск по названию без учета регистра
            var search = TBoxSearch.Text.Trim();
            if (search.Length > 0)
            {
                services = services.Where(p => p.Title.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            //Сортировка по цене с учетом скидки
            services = ComboSortBy.SelectedIndex == 1
                ? services.OrderByDescending(p => p.CostWithDiscount)
                : services.OrderBy(p => p.CostWithDiscount);

            var result = services.ToList();
            LViewServices.ItemsSource = result;
            BlockRecords.Text = $"Показано {result.Count} из {allServices.Count}";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateServices();
        }
    }
}
