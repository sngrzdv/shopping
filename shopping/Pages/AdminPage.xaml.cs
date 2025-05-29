using shopping.AppData;
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

namespace shopping.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        private Frame adminFrame;

        public AdminPage()
        {
            InitializeComponent();
            adminFrame = FindName("AdminFrame") as Frame;

            if (adminFrame == null)
            {
                MessageBox.Show("Ошибка: не найден фрейм для навигации.", "Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        // Обработчик нажатия кнопки Товары
        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            /*adminFrame.Navigate(new AdminProductsPage(adminFrame));*/
        }

        // Обработчик нажатия кнопки Заказы
        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            /*adminFrame.Navigate(new AdminOrdersPage());*/
        }

        // Обработчик нажатия кнопки Пользователи
        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Раздел управления пользователями в разработке.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Обработчик нажатия кнопки Выход
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.frameMain != null)
            {
                AppFrame.frameMain.Navigate(new Autoriz());
            }
        }

        // Другие обработчики (опционально):
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            // Реализуйте обработку добавления товара в корзину
        }

        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            // Реализуйте обработку добавления товара в избранное
        }
    }
}