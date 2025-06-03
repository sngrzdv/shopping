using shop.AppData;
using System.Collections.Generic;
using System.Windows;

namespace shop.Pages
{
    public partial class ChangeRoleDialog : Window
    {
        public role SelectedRole { get; set; }
        public ChangeRoleDialog(List<role> roles, role currentRole)
        {
            InitializeComponent();
            RolesComboBox.ItemsSource = roles;
            RolesComboBox.SelectedItem = currentRole;
            SelectedRole = currentRole;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SelectedRole = (role)RolesComboBox.SelectedItem;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}