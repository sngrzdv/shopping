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
    public partial class AdminPage : Page
    {
        private Frame adminFrame;
        private dressshopEntities db;
        private List<product> allProducts;
        private department selectedDepartment;
        private category selectedCategory;
        private type selectedType;

        public AdminPage()
        {
            InitializeComponent();

            // Добавьте проверку на повторную инициализацию
            if (adminFrame != null) return;

            adminFrame = FindName("AdminFrame") as Frame;
            db = new dressshopEntities();

            LoadDepartments();
            LoadProducts();
        }

        private void LoadDepartments()
        {
            var departments = db.department.ToList();
            DepartmentList.ItemsSource = departments;
        }

        private void LoadCategories(int? departmentId = null)
        {
            // В вашей структуре нет прямой связи между department и category,
            // поэтому будем фильтровать через продукты
            IQueryable<category> categories;

            if (departmentId != null)
            {
                // Получаем только те категории, которые есть у продуктов данного департамента
                // с проверкой на NULL для category
                var categoryIds = db.product
                    .Where(p => p.department.id_department == departmentId && p.category != null)
                    .Select(p => p.category.id_category)
                    .Distinct()
                    .ToList();

                categories = db.category.Where(c => categoryIds.Contains(c.id_category));
            }
            else
            {
                categories = db.category;
            }

            CategoryList.ItemsSource = categories.ToList();
            CategoryExpander.IsExpanded = departmentId != null;
        }

        private void LoadTypes(int? categoryId = null)
        {
            IQueryable<type> types = db.type; // Инициализация по умолчанию

            if (categoryId != null)
            {
                // Получаем только те типы, которые есть у продуктов данной категории
                // с проверкой на NULL для type
                var typeIds = db.product
                    .Where(p => p.category.id_category == categoryId && p.type != null)
                    .Select(p => p.type.id_type)
                    .Distinct()
                    .ToList();

                types = db.type.Where(t => typeIds.Contains(t.id_type));
            }

            TypeList.ItemsSource = types.ToList();
            TypeExpander.IsExpanded = categoryId != null;
        }

        private void LoadProducts()
        {
            // Получаем продукты с включенными связанными данными
            var productsWithDetails = db.product
                .Include("department")
                .Include("category")
                .Include("type")
                .Include("brand")
                .ToList();

            // Обрабатываем null-значения прямо в объектах product
            foreach (var p in productsWithDetails)
            {
                if (string.IsNullOrEmpty(p.description))
                    p.description = "Нет описания";

                if (p.brand == null || string.IsNullOrEmpty(p.brand.brand1))
                    p.brand = new brand { brand1 = "Бренд не указан" };
            }

            allProducts = productsWithDetails;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IQueryable<product> query = db.product.AsQueryable();

            // Применяем фильтры
            if (selectedDepartment != null)
                query = query.Where(p => p.department.id_department == selectedDepartment.id_department);

            // Фильтр по департаменту (с проверкой на NULL)
            if (selectedDepartment != null)
            {
                query = query.Where(p => p.department != null && p.department.id_department == selectedDepartment.id_department);
            }

            // Фильтр по категории (с проверкой на NULL)
            if (selectedCategory != null)
            {
                query = query.Where(p => p.category != null && p.category.id_category == selectedCategory.id_category);
            }

            // Фильтр по типу (с проверкой на NULL)
            if (selectedType != null)
            {
                query = query.Where(p => p.type != null && p.type.id_type == selectedType.id_type);
            }

            // Поиск
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                var searchText = SearchBox.Text.ToLower();
                query = query.Where(p => 
                    p.product1.ToLower().Contains(searchText) || 
                    (p.description != null && p.description.ToLower().Contains(searchText)) ||
                    (p.brand != null && p.brand.brand1.ToLower().Contains(searchText)));
            }

            // Сортировка
            switch ((SortComboBox.SelectedItem as ComboBoxItem)?.Content.ToString())
            {
                case "По названию (А-Я)": 
                    query = query.OrderBy(p => p.product1); 
                    break;
                case "По названию (Я-А)":
                    query = query.OrderByDescending(p => p.product1);
                    break;
                case "По цене (возрастание)":
                    query = query.OrderBy(p => p.price);
                    break;
                case "По цене (убывание)":
                    query = query.OrderByDescending(p => p.price);
                    break;
                default:
                    query = query.OrderBy(p => p.id_product);
                    break;
            }

            listItems.ItemsSource = query.ToList();
        }

        private void DepartmentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedDepartment = DepartmentList.SelectedItem as department;
            selectedCategory = null;
            selectedType = null;
            
            if (selectedDepartment != null)
            {
                LoadCategories(selectedDepartment.id_department);
            }
            else
            {
                CategoryList.ItemsSource = null;
                TypeList.ItemsSource = null;
            }
            
            ApplyFilters();
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCategory = CategoryList.SelectedItem as category;
            selectedType = null;
            
            if (selectedCategory != null)
            {
                LoadTypes(selectedCategory.id_category);
            }
            else
            {
                TypeList.ItemsSource = null;
            }
            
            ApplyFilters();
        }
        private void BtnAllProducts_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем выбранные фильтры
            selectedDepartment = null;
            selectedCategory = null;
            selectedType = null;

            // Очищаем выбор в списках
            DepartmentList.SelectedItem = null;
            CategoryList.SelectedItem = null;
            TypeList.SelectedItem = null;

            // Очищаем поиск
            SearchBox.Text = "";

            // Сбрасываем сортировку на "По умолчанию"
            SortComboBox.SelectedIndex = 0;

            // Загружаем все товары
            LoadProducts();

            // Обновляем списки категорий и типов (чтобы они не зависели от предыдущих фильтров)
            LoadCategories();
            LoadTypes();
        }

        private void TypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedType = TypeList.SelectedItem as type;
            ApplyFilters();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = (sender as Button)?.DataContext as product;
            if (product != null)
            {
                listItems.Visibility = Visibility.Collapsed;
                AdminFrame.Visibility = Visibility.Visible;
                AdminFrame.Navigate(new EditProductPage(product, () =>
                {
                    listItems.Visibility = Visibility.Visible;
                    AdminFrame.Visibility = Visibility.Collapsed;
                }));
                MessageBox.Show($"Редактирование товара: {product.product1}");
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.DataContext as product;

            if (product == null) return;

            MessageBoxResult result = MessageBox.Show(
                $"Вы точно хотите удалить товар: {product.product1}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    db.product.Remove(product);
                    db.SaveChanges();
                    LoadProducts(); // Обновляем список
                    MessageBox.Show("Товар успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Остальные методы остаются без изменений
        private void BtnProducts_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void BtnOrders_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void BtnUsers_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void listItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedProduct = listItems.SelectedItem as product;
            if (selectedProduct != null)
            {
                listItems.Visibility = Visibility.Collapsed;
                AdminFrame.Visibility = Visibility.Visible;
                AdminFrame.Navigate(new ProductDetails(selectedProduct, () =>
                {
                    // Колбэк для возврата
                    listItems.Visibility = Visibility.Visible;
                    AdminFrame.Visibility = Visibility.Collapsed;
                    AdminFrame.Content = null;
                }));
            }
        }
    }
}