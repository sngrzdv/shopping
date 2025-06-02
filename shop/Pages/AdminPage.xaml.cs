using shop.AppData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            db = new dressshopEntities();
            LoadDepartments();
            LoadProducts();
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
            // Отсоединяем все отслеживаемые объекты
            db.Dispose();
            db = new dressshopEntities();

            allProducts = db.product
                .Include(p => p.department)
                .Include(p => p.category)
                .Include(p => p.type)
                .Include(p => p.brand)
                .ToList();

            // Обработка null-значений
            foreach (var p in allProducts)
            {
                p.description = string.IsNullOrEmpty(p.description) ? "Нет описания" : p.description;
                if (p.brand == null) p.brand = new brand { brand1 = "Бренд не указан" };
            }

            ApplyFilters();
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
                    p.brand.brand1.ToLower().Contains(searchText));
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
            var productToEdit = (sender as Button)?.DataContext as product;
            if (productToEdit != null)
            {
                listItems.Visibility = Visibility.Collapsed;
                AdminFrame.Visibility = Visibility.Visible;

                var editPage = new EditProductPage(productToEdit, () =>
                {
                    // Этот код выполнится при возврате со страницы редактирования
                    listItems.Visibility = Visibility.Visible;
                    AdminFrame.Visibility = Visibility.Collapsed;

                    // Обновляем данные
                    RefreshData();
                });

                AdminFrame.Navigate(editPage);
            }
        }

        private void RefreshData()
        {
            // Сохраняем текущие фильтры
            var currentDepartment = selectedDepartment;
            var currentCategory = selectedCategory;
            var currentType = selectedType;
            var currentSearch = SearchBox.Text;
            var currentSort = SortComboBox.SelectedIndex;

            // Полностью перезагружаем данные
            LoadProducts();

            // Восстанавливаем фильтры
            if (currentDepartment != null)
            {
                DepartmentList.SelectedItem = currentDepartment;
                LoadCategories(currentDepartment.id_department);
            }

            if (currentCategory != null)
            {
                CategoryList.SelectedItem = currentCategory;
                LoadTypes(currentCategory.id_category);
            }

            if (currentType != null)
            {
                TypeList.SelectedItem = currentType;
            }

            SearchBox.Text = currentSearch;
            SortComboBox.SelectedIndex = currentSort;

            // Применяем фильтры
            ApplyFilters();
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button)?.DataContext as product;
            if (product == null) return;

            if (MessageBox.Show($"Удалить товар '{product.product1}'?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    db.product.Remove(product);
                    db.SaveChanges();
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var newProduct = new product
            {
                product1 = "Новый товар",
                price = 0,
                quantity = 0,
                description = "Описание",
                image = "/Images/picture.jpg",
                department = db.department.FirstOrDefault(),
                category = db.category.FirstOrDefault(),
                type = db.type.FirstOrDefault(),
                brand = db.brand.FirstOrDefault()
            };

            listItems.Visibility = Visibility.Collapsed;
            AdminFrame.Visibility = Visibility.Visible;

            var editPage = new EditProductPage(newProduct, () =>
            {
                listItems.Visibility = Visibility.Visible;
                AdminFrame.Visibility = Visibility.Collapsed;
                RefreshData();
            });

            AdminFrame.Navigate(editPage);
        }

        private void listItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listItems.SelectedItem is product selectedProduct)
            {
                listItems.Visibility = Visibility.Collapsed;
                AdminFrame.Visibility = Visibility.Visible;
                AdminFrame.Navigate(new ProductDetails(selectedProduct, () =>
                {
                    listItems.Visibility = Visibility.Visible;
                    AdminFrame.Visibility = Visibility.Collapsed;
                }));
            }
        }

        // Остальные методы
        private void BtnProducts_Click(object sender, RoutedEventArgs e) { }
        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу администрирования заказов
            NavigationService.Navigate(new Uri("AdminOrderPage.xaml", UriKind.Relative));
        }
        private void BtnUsers_Click(object sender, RoutedEventArgs e) { }
        private void BtnLogout_Click(object sender, RoutedEventArgs e) { }

        private void BtnAllProducts_Click_1(object sender, RoutedEventArgs e)
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
    }
}