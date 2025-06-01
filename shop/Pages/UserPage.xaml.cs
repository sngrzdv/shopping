using shop.AppData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        private dressshopEntities db;
        private List<product> allProducts;
        private department selectedDepartment;
        private category selectedCategory;
        private type selectedType;
        public UserPage()
        {
            InitializeComponent();
            db = new dressshopEntities();
            LoadDepartments();
            LoadProducts();
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

            // Обновляем списки категорий и типов
            LoadCategories();
            LoadTypes();
        }

        private void LoadDepartments()
        {
            DepartmentList.ItemsSource = db.department.ToList();
        }

        private void LoadCategories(int? departmentId = null)
        {
            try
            {
                IQueryable<category> categories = db.category;

                if (departmentId != null)
                {
                    var categoryIds = db.product
                        .Where(p => p.department.id_department == departmentId && p.category != null)
                        .Select(p => p.category.id_category)
                        .Distinct()
                        .ToList();

                    categories = categories.Where(c => categoryIds.Contains(c.id_category));
                }

                CategoryList.ItemsSource = categories.ToList();
                CategoryExpander.IsExpanded = departmentId != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void LoadTypes(int? categoryId = null)
        {
            try
            {
                IQueryable<type> types = db.type;

                if (categoryId != null)
                {
                    var typeIds = db.product
                        .Where(p => p.category.id_category == categoryId && p.type != null)
                        .Select(p => p.type.id_type)
                        .Distinct()
                        .ToList();

                    types = types.Where(t => typeIds.Contains(t.id_type));
                }

                TypeList.ItemsSource = types.ToList();
                TypeExpander.IsExpanded = categoryId != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            try
            {
                // Получаем все товары с включенными связанными данными
                allProducts = db.product
                .Include(p => p.department)
                .Include(p => p.category)
                .Include(p => p.type)
                .Include(p => p.brand)
                .Where(p => p.quantity > 0) // Фильтр по наличию
                .ToList();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                allProducts = new List<product>(); // Возвращаем пустой список при ошибке
            }
        }

        private void ApplyFilters()
        {
            IQueryable<product> query = db.product.AsQueryable();

            if (selectedDepartment != null)
                query = query.Where(p => p.department.id_department == selectedDepartment.id_department);

            if (selectedCategory != null)
                query = query.Where(p => p.category.id_category == selectedCategory.id_category);

            if (selectedType != null)
                query = query.Where(p => p.type.id_type == selectedType.id_type);

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                var searchText = SearchBox.Text.ToLower();
                query = query.Where(p =>
                    p.product1.ToLower().Contains(searchText) ||
                    p.description.ToLower().Contains(searchText) ||
                    (p.brand != null && p.brand.brand1.ToLower().Contains(searchText)));
            }

            switch ((SortComboBox.SelectedItem as ComboBoxItem)?.Content.ToString())
            {
                case "По названию (А-Я)": query = query.OrderBy(p => p.product1); break;
                case "По названию (Я-А)": query = query.OrderByDescending(p => p.product1); break;
                case "По цене (возрастание)": query = query.OrderBy(p => p.price); break;
                case "По цене (убывание)": query = query.OrderByDescending(p => p.price); break;
                default: query = query.OrderBy(p => p.id_product); break;
            }

            listItems.ItemsSource = query.ToList();
        }

        private void DepartmentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedDepartment = DepartmentList.SelectedItem as department;
            selectedCategory = null;
            selectedType = null;

            if (selectedDepartment != null)
                LoadCategories(selectedDepartment.id_department);
            else
                CategoryList.ItemsSource = TypeList.ItemsSource = null;

            ApplyFilters();
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCategory = CategoryList.SelectedItem as category;
            selectedType = null;

            if (selectedCategory != null)
                LoadTypes(selectedCategory.id_category);
            else
                TypeList.ItemsSource = null;

            ApplyFilters();
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

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button)?.DataContext as product;
            if (product != null)
            {
                // Добавление товара в корзину
                CartManager.AddToCart(product);
                MessageBox.Show($"Товар {product.product1} добавлен в корзину", "Корзина");
            }
        }

        private void listItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listItems.SelectedItem is product selectedProduct)
            {
                NavigationService.Navigate(new ProductDetailsPage(selectedProduct));
            }
        }
    }
}
