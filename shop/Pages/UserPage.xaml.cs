using shop.AppData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
            try
            {
                if ((sender as FrameworkElement)?.DataContext is product selectedProduct)
                {
                    var currentUser = AppConnect.CurrentUser;
                    if (currentUser == null)
                    {
                        MessageBox.Show("Для добавления товаров в корзину необходимо авторизоваться",
                                       "Требуется авторизация",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Warning);
                        NavigationService.Navigate(new Autoriz());
                        return;
                    }

                    // Получаем свежий экземпляр продукта из базы данных
                    var productFromDb = AppConnect.modelOdb.product.Find(selectedProduct.id_product);

                    // Проверяем, есть ли уже такой товар в корзине пользователя
                    var existingItem = AppConnect.modelOdb.basket
                        .FirstOrDefault(b => b.id_user == currentUser.id_user &&
                                           b.id_product == productFromDb.id_product);

                    if (existingItem != null)
                    {
                        // Увеличиваем количество, если товар уже в корзине
                        existingItem.quantity += 1;
                    }
                    else
                    {
                        // Создаем новую запись в корзине
                        var newBasketItem = new basket
                        {
                            id_user = currentUser.id_user,
                            id_product = productFromDb.id_product,
                            quantity = 1
                        };
                        AppConnect.modelOdb.basket.Add(newBasketItem);
                    }

                    AppConnect.modelOdb.SaveChanges();

                    MessageBox.Show($"Товар '{productFromDb.product1}' добавлен в корзину",
                                   "Успешно",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении товара в корзину: {ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }
        private void listItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listItems.SelectedItem is product selectedProduct)
            {
                NavigationService.Navigate(new ProductDetails(selectedProduct, null) );
            }
        }

        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CartPage());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserOrdersPage(null));
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Autoriz());
        }
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
                var existingItem = _db.basket.FirstOrDefault(b => b.id_user == _userId && b.id_product == productId);
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

            public void RemoveFromCart(int basketId)
            {
                var item = _db.basket.FirstOrDefault(b => b.id_basket == basketId && b.id_user == _userId);
                if (item != null)
                {
                    _db.basket.Remove(item);
                    _db.SaveChanges();
                }
            }

            public void UpdateQuantity(int basketId, int newQuantity)
            {
                var item = _db.basket.FirstOrDefault(b => b.id_basket == basketId && b.id_user == _userId);
                if (item != null)
                {
                    item.quantity = newQuantity;
                    _db.SaveChanges();
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

    }
}
