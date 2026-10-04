using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

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

            //Горячие клавиши: Esc - закрыть программу, F11 - обычный размер окна, F12 - на весь экран
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            //Кнопка "Назад" видна только тогда, когда есть куда возвращаться
            FrameMain.Navigated += FrameMain_Navigated;

            //Открытие страницы LoginPage при запуске программы
            FrameMain.Navigate(new Pages.LoginPage());
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.F11:
                    WindowState = WindowState.Normal;
                    break;
                case Key.F12:
                    WindowState = WindowState.Maximized;
                    break;
            }
        }

        private void FrameMain_Navigated(object sender, NavigationEventArgs e)
        {
            btnBack.Visibility = FrameMain.CanGoBack ? Visibility.Visible : Visibility.Hidden;

            //Возврат на страницу авторизации означает выход из учетной записи
            if (e.Content is Pages.LoginPage)
            {
                App.CurrentUser = null;
            }
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
