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
    /// Логика взаимодействия для ProductDetails.xaml
    /// </summary>
    public partial class ProductDetails : Page
    {
        private Action _goBackAction;
        public ProductDetails(product selectedProduct, Action goBackAction)
        {
            InitializeComponent();
            this.DataContext = selectedProduct;
            _goBackAction = goBackAction;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _goBackAction?.Invoke();
        }
    }
}