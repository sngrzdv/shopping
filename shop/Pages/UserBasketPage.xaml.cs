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
        private int userId;
        private dressshopEntities db = new dressshopEntities();

        public UserBasketPage()
        {
            InitializeComponent();

            if (AppConnect.CurrentUser?.id_user == null)
            {
                MessageBox.Show("Для просмотра корзины необходимо авторизоваться",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService?.GoBack();
                return;
            }

            userId = (int)AppConnect.CurrentUser.id_user;
            LoadCartItems();
        }


        // Вложенный класс для работы с корзиной
        public class CartManager
        {
            private readonly dressshopEntities _db;
            private readonly int _userId;

            public CartManager(dressshopEntities db, int userId)
            {
                _db = db;
                _userId = userId;
            }

            public void AddToCart(int productId, int quantity = 1)
            {
                try
                {
                    var existingItem = _db.basket
                        .FirstOrDefault(b => b.id_user == _userId && b.id_product == productId);

                    if (existingItem != null)
                    {
                        existingItem.quantity += quantity;
                    }
                    else
                    {
                        var newItem = new basket
                        {
                            id_user = _userId,
                            id_product = productId,
                            quantity = quantity
                        };
                        _db.basket.Add(newItem);
                    }

                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}");
                }
            }

            public void RemoveFromCart(int basketId)
            {
                try
                {
                    var item = _db.basket.Find(basketId);
                    if (item != null)
                    {
                        _db.basket.Remove(item);
                        _db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении из корзины: {ex.Message}");
                }
            }

            public void UpdateQuantity(int basketId, int newQuantity)
            {
                try
                {
                    var item = _db.basket.Find(basketId);
                    if (item != null)
                    {
                        item.quantity = newQuantity;
                        _db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении количества: {ex.Message}");
                }
            }

            public List<basket> GetUserCart()
            {
                return _db.basket
                    .Include(b => b.product)
                    .Where(b => b.id_user == _userId)
                    .ToList();
            }

            public decimal GetCartTotal()
            {
                return _db.basket
                    .Include(b => b.product)
                    .Where(b => b.id_user == _userId)
                    .Sum(b => b.quantity * b.product.price);
            }
        }

        private void LoadCartItems()
        {
            try
            {
                var cartItems = db.basket
                    .Include(b => b.product)
                    .Where(b => b.id_user == userId)
                    .ToList();

                CartListView.ItemsSource = cartItems;
                UpdateTotalSums();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке корзины: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotalSums()
        {
            // Обновляем суммы для каждого товара
            foreach (var item in CartListView.Items)
            {
                if (item is basket basketItem)
                {
                    var container = CartListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                    if (container != null)
                    {
                        var totalText = FindDescendant<TextBlock>(container, "TotalSumText");
                        if (totalText != null)
                        {
                            totalText.Text = $"Сумма: {basketItem.quantity * basketItem.product.price:N} ₽";
                        }
                    }
                }
            }

            // Обновляем общую сумму корзины
            decimal total = db.basket
                .Include(b => b.product)
                .Where(b => b.id_user == userId)
                .Sum(b => b.quantity * b.product.price);

            CartTotalText.Text = $"Итого: {total:N} ₽";
        }

        // Вспомогательный метод для поиска элементов в визуальном дереве
        private T FindDescendant<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T && (child as FrameworkElement)?.Name == name)
                    return (T)child;

                var result = FindDescendant<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is basket item)
            {
                if (item.quantity > 1)
                {
                    item.quantity--;
                    db.SaveChanges();
                    UpdateTotalSums();

                }
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is basket item)
            {
                item.quantity++;
                db.SaveChanges();
                UpdateTotalSums();
            }
        }

        private void QuantityBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is basket item &&
                int.TryParse(((TextBox)sender).Text, out int newQuantity) &&
                newQuantity > 0)
            {
                item.quantity = newQuantity;
                db.SaveChanges();
                UpdateTotalSums();
            }
            else
            {
                LoadCartItems(); // Восстанавливаем корректное значение
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is basket item)
            {
                db.basket.Remove(item);
                db.SaveChanges();
                LoadCartItems();
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cartItems = db.basket
                    .Include(b => b.product)
                    .Where(b => b.id_user == userId)
                    .ToList();

                if (cartItems.Count == 0)
                {
                    MessageBox.Show("Корзина пуста", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal total = cartItems.Sum(item => item.quantity * item.product.price);

                MessageBox.Show($"Заказ на сумму {total:N} ₽ успешно оформлен!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищаем корзину
                db.basket.RemoveRange(cartItems);
                db.SaveChanges();

                LoadCartItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserPage());
        }
    }
}