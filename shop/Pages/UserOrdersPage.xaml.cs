using shop.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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
    public partial class UserOrdersPage : Page
    {
        private user _currentUser;

        public UserOrdersPage(user currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadOrders();
            UpdateUserInfo();
        }

            private void LoadOrders()
        {
            try
            {
                using (var db = new dressshopEntities())
                {
                    // Получаем все заказы текущего пользователя
                    var orders = db.order
                        .Where(o => o.id_user == _currentUser.id_user)
                        .OrderByDescending(o => o.date)
                        .ToList();

                    OrdersListView.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}");
            }
        }

        private void UpdateUserInfo()
        {
            if (_currentUser == null)
            {
                UserInfoText.Text = "Гость";
                return;
            }

            // Используем безопасное обращение к свойствам
            var surname = _currentUser.surname ?? "";
            var name = _currentUser.name ?? "";
            var familyName = _currentUser.family_name ?? "";

            UserInfoText.Text = $"{surname} {name} {familyName}".Trim();
        }

        private void OrdersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersListView.SelectedItem is order selectedOrder)
            {
                try
                {
                    using (var db = new dressshopEntities())
                    {
                        // Обновляем информацию о выбранном заказе
                        OrderIdText.Text = selectedOrder.id_order.ToString();
                        OrderDateText.Text = selectedOrder.date.ToString("dd.MM.yyyy");
                        OrderStatusText.Text = selectedOrder.order_status.status;

                        // Получаем товары в заказе
                        var orderItems = db.details_order
                            .Where(op => op.id_order == selectedOrder.id_order)
                            .ToList();

                        OrderItemsDataGrid.ItemsSource = orderItems;

                        // Вычисляем общую сумму заказа
                        decimal total = orderItems.Sum(item => item.quantity * item.product.price);
                        TotalAmountText.Text = $"{total:C2}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке деталей заказа: {ex.Message}");
                }
            }
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserPage());
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CartPage());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем страницу заказов
            NavigationService.Navigate(new UserOrdersPage(_currentUser));
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся на страницу авторизации
            NavigationService.Navigate(new Autoriz());
        }
    }
}