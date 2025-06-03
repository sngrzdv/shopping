using System;
using shop.AppData;
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
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace shop.Pages
{
    public partial class AdminOrdersPage : Page
    {
        private dressshopEntities db = new dressshopEntities();
        private order currentOrder;
        public AdminOrdersPage()
        {
            InitializeComponent();
            LoadOrders();
            LoadStatuses();
        }

        private void LoadOrders()
        {
            OrdersListView.ItemsSource = db.order.ToList();
        }

        private void LoadStatuses()
        {
            StatusComboBox.ItemsSource = db.order_status.ToList();
            StatusComboBox.DisplayMemberPath = "status";
            StatusComboBox.SelectedValuePath = "id_status";
        }

        private void OrdersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersListView.SelectedItem is order selectedOrder)
            {
                currentOrder = selectedOrder;
                DisplayOrderDetails(selectedOrder);
            }
        }

        private void DisplayOrderDetails(order order)
        {
            OrderIdText.Text = order.id_order.ToString();
            OrderDateText.Text = order.date.ToString("dd.MM.yyyy");
            StatusComboBox.SelectedValue = order.id_status;

            UserInfoText.Text = $"{order.user.name} ({order.user.email})";

            OrderItemsDataGrid.ItemsSource = order.details_order.ToList();

            decimal total = order.details_order.Sum(item => item.product.price * item.quantity);
            TotalAmountText.Text = total.ToString("C");
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentOrder != null && StatusComboBox.SelectedItem is order_status selectedStatus)
            {
                currentOrder.id_status = selectedStatus.id_status;
                db.SaveChanges();

                // Обновляем отображение в списке заказов
                LoadOrders();
            }
        }

        // Методы для верхнего меню
        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminPage());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            // Уже на странице заказов
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminUsersPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Autoriz());
        }
    }

    // Конвертер для умножения цены на количество
    public class MultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is decimal price && parameter is int quantity)
            {
                return (price * quantity).ToString("C");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}