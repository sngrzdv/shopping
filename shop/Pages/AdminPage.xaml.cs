using shop.AppData;
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

namespace shop.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        private Frame adminFrame;
        private dressshopEntities db; // Экземпляр контекста базы данных
        List<product> products;

        public AdminPage()
        {
            InitializeComponent();
            adminFrame = FindName("AdminFrame") as Frame;

            if (adminFrame == null)
            {
                MessageBox.Show("Ошибка: не найден фрейм для навигации.", "Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Инициализируем подключение к базе данных
            db = new dressshopEntities(); // Создаем экземпляр контекста БД
            LoadProducts(); // Загружаем товары сразу при открытии страницы
        }

        // Метод загрузки товаров
        private void LoadProducts()
        {
            var products = db.product.ToList(); // Выбираем все записи из таблицы Products
            listItems.ItemsSource = products; // Привязываем список товаров к ListView
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
        private void listItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Получаем выбранный продукт
            var selectedProduct = (sender as ListView)?.SelectedItem as product;

            if (selectedProduct != null && adminFrame != null)
            {
                // Открываем новую страницу с подробностями выбранного продукта
                adminFrame.Navigate(new ProductDetails(selectedProduct));
            }
            else
            {
                MessageBox.Show("ATTANTION!!!");
            }
        }
    }
}