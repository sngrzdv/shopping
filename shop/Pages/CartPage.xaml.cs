using shop.AppData;
using System;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace shop.Pages
{
    public partial class CartPage : Page
    {
        private dressshopEntities _context = new dressshopEntities();

        public CartPage()
        {
            InitializeComponent();
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            try
            {
                if (AppConnect.CurrentUser == null)
                {
                    MessageBox.Show("Пользователь не авторизован", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.Navigate(new Autoriz());
                    return;
                }

                var cartItems = _context.basket
                    .Where(b => b.id_user == Flag.IDIDuser)
                    .ToList();

                CartList.ItemsSource = cartItems;
                UpdateTotalPrice(cartItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке корзины: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotalPrice(System.Collections.Generic.List<basket> cartItems)
        {
            decimal total = cartItems.Sum(b => b.TotalPrice);
            tbTotal.Text = $"Итого: {total:N2} ₽";
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as Button).DataContext is basket basketItem)
                {
                    if (MessageBox.Show("Вы уверены, что хотите удалить товар из корзины?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _context.basket.Remove(basketItem);
                        _context.SaveChanges();
                        LoadCartItems();
                        MessageBox.Show("Товар удалён из корзины", "Успешно",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppConnect.CurrentUser == null)
                {
                    MessageBox.Show("Пользователь не авторизован", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.Navigate(new Autoriz());
                    return;
                }

                var cartItems = _context.basket
                    .Where(b => b.id_user == Flag.IDIDuser)
                    .ToList();

                if (!cartItems.Any())
                {
                    MessageBox.Show("Корзина пуста", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (MessageBox.Show("Оформить заказ?", "Подтверждение заказа",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Создаем новый заказ
                    var newOrder = new order
                    {
                        id_user = Flag.IDIDuser,
                        id_status = 1, // Предполагаем, что 1 - это "В обработке"
                        date = DateTime.Now
                    };

                    _context.order.Add(newOrder);
                    _context.SaveChanges();

                    // Добавляем товары в детали заказа
                    foreach (var basketItem in cartItems)
                    {
                        var orderDetail = new details_order
                        {
                            id_order = newOrder.id_order,
                            id_product = basketItem.id_product,
                            quantity = basketItem.quantity,
                            /*price = basketItem.product.price*/
                        };

                        _context.details_order.Add(orderDetail);

                        // Удаляем товар из корзины
                        _context.basket.Remove(basketItem);
                    }

                    _context.SaveChanges();
                    MessageBox.Show("Заказ успешно оформлен!", "Успешно",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadCartItems();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuantityChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var textBox = sender as TextBox;
                if (textBox == null) return;

                var basketItem = textBox.DataContext as basket;
                if (basketItem == null) return;

                if (int.TryParse(textBox.Text, out int quantity) && quantity > 0)
                {
                    basketItem.quantity = quantity;
                    _context.SaveChanges();
                    UpdateTotalPrice(_context.basket
                        .Where(b => b.id_user == basketItem.id_user)
                        .ToList());
                }
                else
                {
                    // Восстановление предыдущего значения
                    textBox.Text = basketItem.quantity.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении количества: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserPage());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserOrdersPage(null));
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                NavigationService.Navigate(new Autoriz());
            }
        }
    }
}