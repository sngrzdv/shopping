using shop.AppData;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace shop.Pages
{
    public partial class UserOrdersPage : Page
    {
        public UserOrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                using (var db = new dressshopEntities())
                {
                    var orders = db.order
                        .Include(o => o.order_status)
                        .Include(o => o.details_order.Select(d => d.product))
                        .Where(o => o.id_user == 1) // Здесь должен быть ID текущего пользователя
                        .OrderByDescending(o => o.date)
                        .ToList();

                    OrdersListView.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message);
            }
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UserPage());
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UserBasketPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Autoriz());
        }
    }
}