using Microsoft.Win32;
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
        private string _imagePath;
        private readonly bool _isNewProduct;

        public EditProductPage(product productToEdit, Action goBackAction = null)
        {
            InitializeComponent();

            // Инициализируем контекст БД в первую очередь
            _db = new dressshopEntities();
            _goBackAction = goBackAction;

            // Проверяем, новый ли товар
            _isNewProduct = productToEdit.id_product == 0;
            _product = _isNewProduct ? CreateNewProduct() : productToEdit;

            DataContext = _product;
            LoadComboBoxData();
            LoadProductImage();
        }

        private product CreateNewProduct()
        {
            var newProduct = new product
            {
                product1 = "Новый товар",
                price = 0,
                quantity = 0,
                description = "Описание",
                image = "/Resources/placeholder.png",
                department = _db.department.FirstOrDefault(),
                category = _db.category.FirstOrDefault(),
                type = _db.type.FirstOrDefault(),
                brand = _db.brand.FirstOrDefault()
            };

            _db.product.Add(newProduct);
            return newProduct;
        }

        private void LoadComboBoxData()
        {
            // Загружаем списки
            DepartmentComboBox.ItemsSource = _db.department.ToList();
            CategoryComboBox.ItemsSource = _db.category.ToList();
            TypeComboBox.ItemsSource = _db.type.ToList();
            BrandComboBox.ItemsSource = _db.brand.ToList();

            // Устанавливаем текущие значения
            DepartmentComboBox.SelectedItem = _product.department;
            CategoryComboBox.SelectedItem = _product.category;
            TypeComboBox.SelectedItem = _product.type;
            BrandComboBox.SelectedItem = _product.brand;
        }

        private void LoadProductImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(_product.image))
                {
                    _imagePath = _product.image; // Сохраняем текущий путь
                    ProductImage.Source = new BitmapImage(new Uri(_product.image));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _imagePath = openFileDialog.FileName;
                    // Обновляем сразу в объекте товара
                    _product.image = _imagePath;
                    ProductImage.Source = new BitmapImage(new Uri(_imagePath));

                    // Для немедленного отображения изменений
                    ProductImage.UpdateLayout();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                if (!string.IsNullOrEmpty(_product.image))
                {
                    _imagePath = _product.image; // Сохраняем текущий путь
                    ProductImage.Source = new BitmapImage(new Uri(_product.image));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Валидация данных (как в предыдущем примере)
            if (!ValidateInput()) return;

            try
            {
                // Обновляем данные
                _product.product1 = NameBox.Text;
                _product.price = decimal.Parse(PriceBox.Text);
                _product.quantity = int.Parse(QuantityBox.Text);
                _product.description = DescBox.Text;
                _product.department = (department)DepartmentComboBox.SelectedItem;
                _product.category = (category)CategoryComboBox.SelectedItem;
                _product.type = (type)TypeComboBox.SelectedItem;
                _product.brand = (brand)BrandComboBox.SelectedItem;

                // Обновляем фото (если выбрано новое)
                if (!string.IsNullOrEmpty(_imagePath))
                {
                    _product.image = _imagePath;
                }

                _db.SaveChanges();
                MessageBox.Show("Товар успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _goBackAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            // Добавьте проверки для всех полей
            if (DepartmentComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите департамент!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
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