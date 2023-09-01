using Microsoft.Win32;
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

namespace CarService.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditServicePage.xaml
    /// </summary>
    public partial class AddEditServicePage : Page
    {
        private Entities.Service _currentService = null;
        
        //Поле, хранящее массив байтов изображения
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
            if (_currentService.Discount > 0)
            {
                TBoxDiscount.Text = (_currentService.Discount.Value * 100).ToString();
            }

            if (_currentService.MainImage != null)
            {
                ImageService.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(_currentService.MainImage);
            }

        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            //Обработка выбора файла картинки
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image |*.png; *.jpg; *.jpeg";
            if (ofd.ShowDialog() == true)
            {
                _mainImageData = File.ReadAllBytes(ofd.FileName);
                ImageService.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(_mainImageData);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var errorMessage = CheckErrors();
            if (errorMessage.Length > 0)
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                //При добавлении услуги, введённые данные подставляются в поля и сохраняются в БД
                if (_currentService == null)
                {
                    var service = new Entities.Service
                    {
                        Title = TBoxTitle.Text,
                        Cost = decimal.Parse(TBoxCost.Text),
                        DurationInSeconds = int.Parse(TBoxDuration.Text) * 60,
                        Description = TBoxDescription.Text,
                        Discount = string.IsNullOrWhiteSpace(TBoxDiscount.Text) ? 0 : double.Parse(TBoxDiscount.Text) / 100,
                        MainImage = _mainImageData
                    };
                    App.Context.Service.Add(service);
                    App.Context.SaveChanges();
                    MessageBox.Show("Добавление успешно выполнено");
                }

                //При редактировании услуги, на основе введённых данных происходит обновление и сохранение в БД
                else
                {
                    _currentService.Title = TBoxTitle.Text;
                    _currentService.Cost = decimal.Parse(TBoxCost.Text);
                    _currentService.DurationInSeconds = int.Parse(TBoxDuration.Text) * 60;
                    _currentService.Description = TBoxDescription.Text;
                    _currentService.Discount = string.IsNullOrWhiteSpace(TBoxDiscount.Text) ? 0 : double.Parse(TBoxDiscount.Text) / 100;
                    if (_mainImageData != null)
                    {
                        _currentService.MainImage = _mainImageData;
                    }
                    App.Context.SaveChanges();
                    MessageBox.Show("Редактирование успешно выполнено");
                }
                NavigationService.GoBack();
            }

        }

        private string CheckErrors()
        {
            var errorBuilder = new StringBuilder();

            //Проверка на заполнение наименования услуги
            if (string.IsNullOrWhiteSpace(TBoxTitle.Text))
                errorBuilder.AppendLine("Названиеуслуги обязательно для заполнения");

            //Проверка новой услуги с уже существующими в БД
            var serviceFromDB = App.Context.Service.ToList().FirstOrDefault(P => P.Title.ToLower() == TBoxTitle.Text.ToLower());
            if (serviceFromDB != null && serviceFromDB != _currentService)
            {
                errorBuilder.AppendLine("Такая услуга уже есть в базе данных;");
            }

            
            decimal cost = 0;
            //Проверка неотрицательности цены за услугу
            if (decimal.TryParse(TBoxCost.Text, out cost) == false || cost <= 0)
            {
                errorBuilder.AppendLine("Стоимость услуги должна быть положительным числом;");
            }


            int durationInMinutes = 0;
            //Проверка неотрицательности длительности услуги
            if (int.TryParse(TBoxDuration.Text, out durationInMinutes) == false || durationInMinutes > 240 || durationInMinutes <= 0)
            {
                errorBuilder.AppendLine("Длительность оказания услуги должна быть положительным " + "числом (не больше, чем 4 часа);");
            }

            //Проверка заполения скидки
            if (!string.IsNullOrEmpty(TBoxDiscount.Text))
            {
                int discount = 0;
                //Проверка на принадлежность к диапазону от 0 до 100
                if (int.TryParse(TBoxDiscount.Text, out discount) == false || discount < 0 || discount > 100)
                {
                    errorBuilder.AppendLine("Размер скидки - целое число в диапазоне от 0 до 100%;");
                }
            }

            //Просто вывод ошибок, которые нужно исправить
            if (errorBuilder.Length > 0)
            {
                errorBuilder.Insert(0, "Устраните следующие ошибки:\n");
            }
            return errorBuilder.ToString();
        }
    }
}