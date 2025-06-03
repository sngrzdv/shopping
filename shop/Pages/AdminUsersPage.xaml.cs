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
    /// Логика взаимодействия для AdminUsersPage.xaml
    /// </summary>
    public partial class AdminUsersPage : Page
    {
        private dressshopEntities _db;
        public List<role> AllRoles { get; set; }
        public AdminUsersPage()
        {
            InitializeComponent();
            _db = new dressshopEntities();
            LoadData();
            DataContext = this;

            // Подписываемся на событие Unloaded
            this.Unloaded += AdminUsersPage_Unloaded;
        }

        private void AdminUsersPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _db?.Dispose();
        }

        private void LoadData()
        {
            _db.user.Include(u => u.role).Include(u => u.order).Load();
            _db.role.Load();

            AllRoles = _db.role.Local.ToList();
            UsersGrid.ItemsSource = _db.user.Local;
        }

        private void EditRole_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is user selectedUser)
            {
                // Создаем диалог для изменения роли
                var dialog = new ChangeRoleDialog(AllRoles, selectedUser.role)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        selectedUser.id_role = dialog.SelectedRole.id_role;
                        selectedUser.role = dialog.SelectedRole;
                        _db.SaveChanges();

                        // Обновляем отображение
                        UsersGrid.Items.Refresh();

                        MessageBox.Show("Роль пользователя успешно изменена!", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при изменении роли: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        _db.Entry(selectedUser).Reload();
                    }
                }
            }
        }

        private void RoleComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is user user)
            {
                comboBox.SelectedValue = user.id_role;
            }
        }

        private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersGrid.SelectedItem is user selectedUser && _db.Entry(selectedUser).State == EntityState.Modified)
            {
                try
                {
                    _db.SaveChanges();
                    MessageBox.Show("Роль пользователя успешно изменена!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при изменении роли: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    _db.Entry(selectedUser).Reload();
                }
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var userToDelete = _db.user.FirstOrDefault(u => u.id_user == userId);
                if (userToDelete != null)
                {
                    // Подтверждение удаления
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя {userToDelete.surname} {userToDelete.name}?",
                                              "Подтверждение удаления",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Удаляем пользователя
                            _db.user.Remove(userToDelete);
                            _db.SaveChanges();

                            // Обновляем список
                            LoadData();

                            MessageBox.Show("Пользователь успешно удалён", "Успех",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        // Обработчики кнопок меню
        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminPage());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminOrdersPage());
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            // Уже на этой странице, можно обновить данные
            LoadData();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Логика выхода
            NavigationService.Navigate(new Autoriz());
        }
    }
}