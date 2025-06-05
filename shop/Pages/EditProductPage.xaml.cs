using Microsoft.Win32;
using shop.AppData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
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

            _db = new dressshopEntities();
            _goBackAction = goBackAction;
            _isNewProduct = productToEdit.id_product == 0;

            // Если товар новый - создаем, иначе загружаем текущий из БД
            _product = _isNewProduct ? CreateNewProduct() : _db.product.Find(productToEdit.id_product);

            if (_product == null)
            {
                MessageBox.Show("Товар не найден в базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.GoBack();
                return;
            }

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
            DepartmentComboBox.ItemsSource = _db.department.ToList();
            CategoryComboBox.ItemsSource = _db.category.ToList();
            TypeComboBox.ItemsSource = _db.type.ToList();
            BrandComboBox.ItemsSource = _db.brand.ToList();

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
                    _imagePath = _product.image;
                    ImagePathTextBox.Text = _product.image; // Устанавливаем путь в текстовое поле

                    // Проверяем, является ли путь абсолютным
                    if (File.Exists(_product.image) || Uri.IsWellFormedUriString(_product.image, UriKind.Absolute))
                    {
                        ProductImage.Source = new BitmapImage(new Uri(_product.image, UriKind.RelativeOrAbsolute));
                    }
                    else
                    {
                        // Попробуем найти изображение относительно исполняемого файла
                        string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _product.image.TrimStart('/'));
                        if (File.Exists(fullPath))
                        {
                            ProductImage.Source = new BitmapImage(new Uri(fullPath));
                        }
                    }
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
                    ImagePathTextBox.Text = _imagePath; // Обновляем текстовое поле
                    ProductImage.Source = new BitmapImage(new Uri(_imagePath));
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
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название товара!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (число > 0)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество (целое число ≥ 0)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInput()) return;

            try
            {
                // Обновляем данные
                _product.product1 = NameBox.Text;
                _product.price = price;
                _product.quantity = quantity;
                _product.description = DescBox.Text;

                // Убедимся, что навигационные свойства установлены правильно
                _product.id_department = ((department)DepartmentComboBox.SelectedItem).id_department;
                _product.id_category = ((category)CategoryComboBox.SelectedItem).id_category;
                _product.id_type = ((type)TypeComboBox.SelectedItem).id_type;
                _product.id_brand = ((brand)BrandComboBox.SelectedItem).id_brand;

                // Обновляем фото
                if (!string.IsNullOrEmpty(ImagePathTextBox.Text))
                {
                    _product.image = ImagePathTextBox.Text;
                }

                // Явно указываем, что объект изменен
                _db.Entry(_product).State = _isNewProduct ? EntityState.Added : EntityState.Modified;

                _db.SaveChanges();
                MessageBox.Show("Товар успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _goBackAction?.Invoke();
            }
            catch (DbUpdateException ex)
            {
                string errorMessage = "Ошибка сохранения в БД:\n";

                // Получаем все внутренние исключения
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    errorMessage += innerEx.Message + "\n";
                    innerEx = innerEx.InnerException;
                }

                MessageBox.Show(errorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);

                // Откатываем изменения
                _db.Entry(_product).Reload();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\n{ex.InnerException?.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (DepartmentComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите департамент!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (BrandComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите бренд!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Отменяем изменения для нового товара
            if (_isNewProduct)
            {
                _db.product.Remove(_product);
            }
            else
            {
                // Отменяем изменения для существующего товара
                _db.Entry(_product).Reload();
            }

            _goBackAction?.Invoke();
            NavigationService?.GoBack();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;
            }
        }

        private void IntegerValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void ImagePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}