using shop.AppData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
                MessageBox.Show("Для просмотра корзины необходимо авторизоваться", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService?.GoBack();
                return;
            }

            userId = (int)AppConnect.CurrentUser.id_user;
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            try
            {
                var cartItems = db.basket
                    .Include(b => b.product)
                    .Where(b => b.id_user == userId)
                    .ToList();

                CartListView.ItemsSource = cartItems.Select(x => new BasketItemViewModel(x)).ToList();
                UpdateCartTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке корзины: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCartTotal()
        {
            decimal total = db.basket
                .Include(b => b.product)
                .Where(b => b.id_user == userId)
                .Sum(b => b.quantity * b.product.price);

            CartTotalText.Text = $"Итого: {total:N} ₽";
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is basket item)
            {
                if (item.quantity > 1)
                {
                    item.quantity--;
                    db.SaveChanges();
                    UpdateCartTotal();
                }
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is basket item)
            {
                item.quantity++;
                db.SaveChanges();
                UpdateCartTotal();
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
                UpdateCartTotal();
            }
            else
            {
                LoadCartItems(); // Восстановить если ошибка
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

                if (!cartItems.Any())
                {
                    MessageBox.Show("Корзина пуста", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal total = cartItems.Sum(item => item.quantity * item.product.price);
                MessageBox.Show($"Заказ на сумму {total:N} ₽ успешно оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                db.basket.RemoveRange(cartItems);
                db.SaveChanges();
                LoadCartItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserPage());
        }
        public class BasketItemViewModel : INotifyPropertyChanged
        {
            public basket BasketItem { get; }
            public product Product => BasketItem.product;

            public event PropertyChangedEventHandler PropertyChanged;

            public BasketItemViewModel(basket item)
            {
                BasketItem = item;
            }

            public int Quantity
            {
                get => BasketItem.quantity;
                set
                {
                    if (BasketItem.quantity != value)
                    {
                        BasketItem.quantity = value;
                        OnPropertyChanged(nameof(Quantity));
                        OnPropertyChanged(nameof(TotalPrice));
                    }
                }
            }

            public decimal TotalPrice => Quantity * (Product?.price ?? 0);

            protected void OnPropertyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
