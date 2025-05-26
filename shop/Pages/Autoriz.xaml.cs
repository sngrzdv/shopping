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
    /// Логика взаимодействия для Autoriz.xaml
    /// </summary>
    public partial class Autoriz : Page
    {
        private string _lastEnteredEmail = string.Empty;

        public Autoriz()
        {
            InitializeComponent(); // Обязательная инициализация компонентов
            AppConnect.modelOdb = new dressshopEntities(); // Подключение модели базы данных
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var email = txtEmail.Text.Trim();
            var password = txtPassword.Password;

            var user = AppConnect.modelOdb.user
                .FirstOrDefault(u => u.email == email && u.password == password);

            if (user != null)
            {
                AppConnect.CurrentUser = user; // Сохраняем пользователя

                if (user.id_role == 3) // админ
                    NavigationService.Navigate(new AdminPage());
                /*else // обычный пользователь
                    NavigationService.Navigate(new UserPage());*/
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            _lastEnteredEmail = txtEmail.Text.Trim(); // Сохранение введенного email
            //frmMain.Navigate(new Registr(_lastEnteredEmail)); // Переход на регистрацию
            NavigationService.Navigate(new Registr());
        }
    }
}