using shop.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для Registr.xaml
    /// </summary>
    public partial class Registr : Page
    {
        public Registr(string email = null)
        {
            InitializeComponent();
            // Устанавливаем переданный email (если есть)
            if (!string.IsNullOrEmpty(email))
            {
                txtEmail.Text = email;
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                    string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Password) ||
                    string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
                {
                    ShowError("Заполните все обязательные поля!");
                    return;
                }

                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    ShowError("Пароли не совпадают!");
                    return;
                }

                if (txtPassword.Password.Length < 6)
                {
                    ShowError("Пароль должен содержать минимум 6 символов!");
                    return;
                }

                if (!IsValidEmail(txtEmail.Text))
                {
                    ShowError("Введите корректный email адрес!");
                    return;
                }

                if (AppConnect.modelOdb.user.Any(u => u.email == txtEmail.Text))
                {
                    ShowError("Пользователь с таким email уже существует!");
                    return;
                }

                var newUser = new user
                {
                    surname = txtLastName.Text.Trim(),
                    name = txtFirstName.Text.Trim(),
                    family_name = string.IsNullOrWhiteSpace(txtPatronymic.Text) ? null : txtPatronymic.Text.Trim(),
                    email = txtEmail.Text.Trim(),
                    password = txtPassword.Password,
                    id_role = 3
                };

                AppConnect.modelOdb.user.Add(newUser);
                AppConnect.modelOdb.SaveChanges();

                MessageBox.Show("Регистрация прошла успешно!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new Autoriz());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new Autoriz());
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}