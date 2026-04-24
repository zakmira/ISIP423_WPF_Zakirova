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
using static WpfApp17.MainWindow;
using WpfApp17.Pages;

namespace WpfApp17
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _currentUserId = 0;
        private string _currentUserRole = "";

        public MainWindow()
        {
            InitializeComponent();
            CheckLoginStatus();

            MainFrame.Navigate(new Pages.Start());
        }

        public void CheckLoginStatus()
        {
            if (Core.CurrentUserId > 0)
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.Id == Core.CurrentUserId);
                if (user != null)
                {
                    Account.Visibility = Visibility.Visible;
                    Login.Visibility = Visibility.Collapsed;
                    Logout.Visibility = Visibility.Visible;
                }
            }
            else
            {
                Account.Visibility = Visibility.Collapsed;
                Login.Visibility = Visibility.Visible;
                Logout.Visibility = Visibility.Collapsed;
            }

            if (Core.CurrentUserId > 0)
            {
                _currentUserId = Core.CurrentUserId;
                _currentUserRole = Core.CurrentUserRole;

                var user = Core.Context.Users.FirstOrDefault(u => u.Id == _currentUserId);
                if (user != null)
                {
                    Account.Visibility = Visibility.Visible;
                    Login.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                Account.Visibility = Visibility.Collapsed;
                Login.Visibility = Visibility.Visible;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var loginPage = new Pages.Login();
            loginPage.ReturnAction = () => CheckLoginStatus();
            MainFrame.Navigate(loginPage);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Core.CurrentUserId = 0;
                Core.CurrentUserRole = "";

                if (Core.CartItems != null)
                {
                    Core.CartItems.Clear();
                }

                CheckLoginStatus();

                MainFrame.Navigate(new Pages.Start());

                MessageBox.Show("До свидания");
            }
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.Client.ProductsPage());
        }

        private void Account_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0) return;

            if (_currentUserRole == "Client")
            {
                MainFrame.Navigate(new Pages.Client.Client());
            }
            else if (_currentUserRole == "Master")
            {
                MainFrame.Navigate(new Pages.Master.Master());
            }
            else if (_currentUserRole == "Manager")
            {
                MainFrame.Navigate(new Pages.Manager.ManagerPage());
            }
            else if (_currentUserRole == "Admin")
            {
                MainFrame.Navigate(new Pages.Admin.Admin());
            }
        }
    }
}
