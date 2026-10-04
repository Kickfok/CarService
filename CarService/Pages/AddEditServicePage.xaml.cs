using Microsoft.Win32;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarService.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditServicePage.xaml
    /// </summary>
    public partial class AddEditServicePage : Page
    {
        // Максимальная длительность услуги в минутах.
        private const int MaxDurationInMinutes = 240;

        private readonly Entities.Service _currentService;

        //Поле, хранящее массив байтов выбранного изображения
        private byte[] _mainImageData;

        public AddEditServicePage()
        {
            InitializeComponent();
        }

        public AddEditServicePage(Entities.Service service)
        {
            InitializeComponent();

            _currentService = service;
            Title = "Редактирование услуги";
            TBoxTitle.Text = _currentService.Title;
            TBoxCost.Text = _currentService.Cost.ToString("N2");
            TBoxDuration.Text = (_currentService.DurationInSeconds / 60).ToString();
            TBoxDescription.Text = _currentService.Description;
            if (_currentService.HasDiscount)
            {
                //Скидка хранится долей, в поле показываются целые проценты
                TBoxDiscount.Text = Math.Round(_currentService.DiscountPercent).ToString();
            }

            if (_currentService.MainImage != null)
            {
                ImageService.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(_currentService.MainImage);
            }
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            //Обработка выбора файла картинки
            var ofd = new OpenFileDialog
            {
                Filter = "Изображения (*.png; *.jpg; *.jpeg)|*.png;*.jpg;*.jpeg"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var data = File.ReadAllBytes(ofd.FileName);
                    ImageService.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(data);
                    _mainImageData = data;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось открыть изображение.\n\n" + ex.Message, "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var errorMessage = CheckErrors();
            if (errorMessage.Length > 0)
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var service = _currentService ?? new Entities.Service();

            //Перенос введенных данных в услугу
            service.Title = TBoxTitle.Text.Trim();
            service.Cost = decimal.Parse(TBoxCost.Text);
            service.DurationInSeconds = int.Parse(TBoxDuration.Text) * 60;
            service.Description = TBoxDescription.Text;
            service.Discount = string.IsNullOrWhiteSpace(TBoxDiscount.Text) ? 0 : int.Parse(TBoxDiscount.Text) / 100.0;
            if (_mainImageData != null)
            {
                service.MainImage = _mainImageData;
            }

            if (_currentService == null)
            {
                App.Context.Service.Add(service);
            }

            try
            {
                App.Context.SaveChanges();
            }
            catch (Exception ex)
            {
                //Откат: новая услуга убирается из контекста, у существующей восстанавливаются значения из БД
                var entry = App.Context.Entry(service);
                if (entry.State == EntityState.Added)
                {
                    entry.State = EntityState.Detached;
                }
                else
                {
                    entry.Reload();
                }
                MessageBox.Show("Не удалось сохранить услугу.\n\n" + ex.GetBaseException().Message, "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(_currentService == null ? "Услуга добавлена." : "Изменения сохранены.", "Готово",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService.GoBack();
        }

        private string CheckErrors()
        {
            var errorBuilder = new StringBuilder();
            var title = TBoxTitle.Text.Trim();

            //Проверка на заполнение наименования услуги
            if (title.Length == 0)
            {
                errorBuilder.AppendLine("Название услуги обязательно для заполнения;");
            }
            else
            {
                //Проверка на совпадение названия с другой услугой из БД
                var lowerTitle = title.ToLower();
                var serviceFromDB = App.Context.Service.FirstOrDefault(p => p.Title.ToLower() == lowerTitle);
                if (serviceFromDB != null && serviceFromDB != _currentService)
                {
                    errorBuilder.AppendLine("Такая услуга уже есть в базе данных;");
                }
            }

            //Проверка, что цена - положительное число
            decimal cost;
            if (!decimal.TryParse(TBoxCost.Text, out cost) || cost <= 0)
            {
                errorBuilder.AppendLine("Стоимость услуги должна быть положительным числом;");
            }

            //Проверка, что длительность - положительное число не больше MaxDurationInMinutes
            int durationInMinutes;
            if (!int.TryParse(TBoxDuration.Text, out durationInMinutes) || durationInMinutes <= 0 ||
                durationInMinutes > MaxDurationInMinutes)
            {
                errorBuilder.AppendLine("Длительность оказания услуги должна быть положительным числом " +
                                        "(не больше, чем 4 часа);");
            }

            //Скидка необязательна, но если указана - целое число от 0 до 100
            if (!string.IsNullOrWhiteSpace(TBoxDiscount.Text))
            {
                int discount;
                if (!int.TryParse(TBoxDiscount.Text, out discount) || discount < 0 || discount > 100)
                {
                    errorBuilder.AppendLine("Размер скидки - целое число в диапазоне от 0 до 100%;");
                }
            }

            if (errorBuilder.Length > 0)
            {
                errorBuilder.Insert(0, "Устраните следующие ошибки:\n");
            }
            return errorBuilder.ToString();
        }
    }
}
