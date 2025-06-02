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
using System.Data.Entity;

namespace shop.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminOrderPage.xaml
    /// </summary>
    public partial class AdminOrderPage : Page
    {

        public AdminOrderPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var currentUser = AppConnect.CurrentUser;

                var orders = AppConnect.modelOdb.order
                    .Where(s => s.id_user == currentUser.id_user)
                    .OrderByDescending(s => s.date)
                    .ToList();

                ordersList.ItemsSource = orders;

                tbCounter.Text = orders.Any()
                    ? $"Оформленных заказов: {orders.Count}"
                    : "У вас нет оформленных заказов.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

