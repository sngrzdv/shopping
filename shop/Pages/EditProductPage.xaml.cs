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
    /// Логика взаимодействия для EditProductPage.xaml
    /// </summary>
    public partial class EditProductPage : Page
    {
        private product _product;
        private dressshopEntities _db;
        private Action _goBackAction;

        public EditProductPage(product productToEdit, Action goBackAction = null)
        {
            InitializeComponent();
            _product = productToEdit;
            _db = new dressshopEntities();
            _goBackAction = goBackAction;
            DataContext = _product;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название товара!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка цены (должна быть положительным числом)
            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (число > 0)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка количества (должно быть целым числом ≥ 0)
            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество (целое число ≥ 0)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Обновляем данные
                _product.product1 = NameBox.Text;
                _product.price = price;
                _product.quantity = quantity;
                _product.description = DescBox.Text;

                _db.SaveChanges();
                MessageBox.Show("Изменения сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Возвращаемся назад
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _goBackAction?.Invoke();
            NavigationService?.GoBack();
        }

        // Разрешает ввод только чисел (с точкой для decimal)
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true; // Блокируем ввод
            }
        }

        // Разрешает ввод только целых чисел
        private void IntegerValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
    }
}