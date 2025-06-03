using shop.AppData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
    public partial class UserBasketPage : Page
    {
        private dressshopEntities db = new dressshopEntities();
        private int currentUserId; // ID текущего пользователя
        private ObservableCollection<CartItemViewModel> _cartItems;

        public UserBasketPage(int userId)
        {
            InitializeComponent();
            currentUserId = userId;
            _cartItems = new ObservableCollection<CartItemViewModel>();
            CartItemsControl.ItemsSource = _cartItems;
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            _cartItems.Clear();

            var basketItems = db.basket
                .Where(b => b.id_user == currentUserId)
                .Include(b => b.product)
                .ToList();

            foreach (var item in basketItems)
            {
                _cartItems.Add(new CartItemViewModel(item));
            }

            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            decimal total = _cartItems.Sum(item => item.ItemTotal);
            TotalAmountText.Text = total.ToString("C");
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            var itemsToRemove = db.basket.Where(b => b.id_user == currentUserId).ToList();
            db.basket.RemoveRange(itemsToRemove);
            db.SaveChanges();
            LoadCartItems();
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int basketId)
            {
                var item = db.basket.Find(basketId);
                if (item != null)
                {
                    item.quantity++;
                    db.SaveChanges();
                    LoadCartItems();
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int basketId)
            {
                var item = db.basket.Find(basketId);
                if (item != null)
                {
                    if (item.quantity > 1)
                    {
                        item.quantity--;
                        db.SaveChanges();
                    }
                    else
                    {
                        db.basket.Remove(item);
                        db.SaveChanges();
                    }
                    LoadCartItems();
                }
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, есть ли товары в корзине
                var cartItems = db.basket
                    .Where(b => b.id_user == currentUserId)
                    .Include(b => b.product)
                    .ToList();

                if (!cartItems.Any())
                {
                    MessageBox.Show("Ваша корзина пуста!");
                    return;
                }

                // Создаем новый заказ
                var order = new AppData.order
                {
                    id_user = currentUserId,
                    date = DateTime.Now,
                    order_status = "В сборке"
                };

                db.order.Add(order);
                db.SaveChanges();

                // Добавляем товары из корзины в детали заказа
                foreach (var item in cartItems)
                {
                    var detail = new AppData.details_order
                    {
                        id_order = order.id_order,
                        id_product = item.id_product,
                        quantity = item.quantity
                        // price не указываем, если его нет в модели
                        // Или используем цену из связанного товара:
                        // price = item.product.price (если добавите поле в модель)
                    };

                    db.details_order.Add(detail);
                }

                // Очищаем корзину
                db.basket.RemoveRange(cartItems);
                db.SaveChanges();

                MessageBox.Show($"Заказ №{order.id_order} успешно оформлен!");
                LoadCartItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}");
            }
        }

        // Обработчики для верхнего меню
        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserPage());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            /*NavigationService.Navigate(new OrdersPage(currentUserId));*/
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            /*NavigationService.Navigate(new UsersPage());*/
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Autoriz());
        }
    }
}