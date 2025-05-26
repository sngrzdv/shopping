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
        private Frame _adminFrame;

        public AdminPage()
        {
            InitializeComponent();
            _adminFrame = this.FindName("AdminFrame") as Frame;

            // Добавляем проверку на null
            if (_adminFrame == null)
            {
                MessageBox.Show("Ошибка: не найден фрейм для навигации");
                return;
            }

            /*_adminFrame.Navigate(new AdminProductsPage(_adminFrame));*/
        }

        // Остальные методы остаются без изменений
        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            /*_adminFrame.Navigate(new AdminProductsPage(_adminFrame));*/
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            /*_adminFrame?.Navigate(new AdminOrdersPage());*/
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            //    // Временная заглушка, пока нет реализации AdminUsersPage
            //    MessageBox.Show("Раздел управления пользователями в разработке");

            //    // Если страница существует:
            //    // _adminFrame?.Navigate(new AdminUsersPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.frameMain != null)
            {
                AppFrame.frameMain.Navigate(new Autoriz());
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is product selectedItem)
            {
                var user = AppConnect.CurrentUser;
                if (user == null)
                {
                    MessageBox.Show("Необходима авторизация.");
                    return;
                }

                // Проверка, есть ли уже товар в корзине
                var existingCart = AppConnect.modelOdb.basket
                    .FirstOrDefault(c => c.id_user == user.id_user && c.id_product == selectedItem.id_product);

                if (existingCart != null)
                {
                    existingCart.quantity += 1; // увеличиваем количество
                }
                else
                {
                    var cart = new basket
                    {
                        id_user = user.id_user,
                        id_product = selectedItem.id_product,
                        quantity = 1
                    };
                    AppConnect.modelOdb.basket.Add(cart);
                }

                AppConnect.modelOdb.SaveChanges();
                MessageBox.Show($"Товар '{selectedItem.product1}' добавлен в корзину.");
            }
        }
    }
}