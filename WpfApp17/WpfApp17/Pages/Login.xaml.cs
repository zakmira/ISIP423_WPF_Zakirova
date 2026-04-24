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

namespace WpfApp17.Pages
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Action ReturnAction { get; set; }

        public Login()
        {
            InitializeComponent();
        }

        private void LoginB_Click(object sender, RoutedEventArgs e)
        {
            string login = EnterLogin.Text.Trim();
            string password = Password.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            var user = Core.Context.Users
                .FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль");
                return;
            }

            if (user.IsFrozen == true)
            {
                MessageBox.Show("Ваш аккаунт заморожен");
                return;
            }

            Core.CurrentUserId = user.Id;
            Core.CurrentUserRole = user.Role;

            MessageBox.Show($"Добро пожаловать, {user.FullName}!");

            ReturnAction?.Invoke();

            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}