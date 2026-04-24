using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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

namespace WpfApp17.Pages.Admin
{
    /// <summary>
    /// Логика взаимодействия для Admin.xaml
    /// </summary>
    public partial class Admin : Page
    {
        private List<string> _roles = new List<string> { "Client", "Master", "Manager", "Admin" };

        public Admin()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = Core.Context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToList();

            UsersList.ItemsSource = users;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddUser();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                LoadUsers();
                MessageBox.Show("Пользователь добавлен");
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.Tag as Users;

            if (user != null)
            {
                var dialog = new AddUser(user);
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    LoadUsers();
                    MessageBox.Show("Данные пользователя обновлены");
                }
            }
        }

        private void Role_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            var user = combo?.Tag as Users;

            if (user != null)
            {
                string newRole = combo.SelectedItem as string;

                if (newRole != null && user.Role != newRole)
                {
                    if (user.Role == "Admin" && Core.CurrentUserId != user.Id)
                    {
                        MessageBox.Show("Нельзя изменить роль другого администратора");
                        LoadUsers();
                        return;
                    }

                    var result = MessageBox.Show($"Изменить роль пользователя {user.FullName} с {user.Role} на {newRole}?",
                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        user.Role = newRole;
                        Core.Context.SaveChanges();
                        LoadUsers();
                        MessageBox.Show("Роль изменена");
                    }
                    else
                    {
                        LoadUsers();
                    }
                }
            }
        }

        private void ToggleFreeze_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.Tag as Users;

            if (user != null)
            {
                if (user.Id == Core.CurrentUserId)
                {
                    MessageBox.Show("Нельзя заморозить самого себя");
                    return;
                }

                string action = user.IsFrozen ? "разморозить" : "заморозить";
                var result = MessageBox.Show($"{action} пользователя {user.FullName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    user.IsFrozen = !user.IsFrozen;
                    Core.Context.SaveChanges();
                    LoadUsers();

                    if (user.IsFrozen && Core.CurrentUserId == user.Id)
                    {
                        Core.CurrentUserId = 0;
                        Core.CurrentUserRole = "";

                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.MainFrame.Navigate(new Pages.Start());
                            mainWindow.CheckLoginStatus();
                        }

                        MessageBox.Show($"Пользователь {user.FullName} заморожен. Вы были разлогинены.");
                    }
                    else
                    {
                        MessageBox.Show("Пользователь заморожен/разморожен");
                    }
                }
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.Tag as Users;

            if (user != null)
            {

                if (user.Id == Core.CurrentUserId)
                {
                    MessageBox.Show("Нельзя удалить самого себя");
                    return;
                }

                var activeAppointments = Core.Context.Appointments
                    .Where(a => a.ClientId == user.Id && a.Status != "Completed" && a.Status != "Cancelled")
                    .ToList();

                if (activeAppointments.Any())
                {
                    MessageBox.Show($"Нельзя удалить пользователя {user.FullName}, так как у него есть активные записи");
                    return;
                }

                var activeOrders = Core.Context.Orders
                    .Where(o => o.ClientId == user.Id && o.Status != "Completed" && o.Status != "Cancelled")
                    .ToList();

                if (activeOrders.Any())
                {
                    MessageBox.Show($"Нельзя удалить пользователя {user.FullName}, так как у него есть активные заказы");
                    return;
                }

                var result = MessageBox.Show($"Удалить пользователя {user.FullName}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var userAppointments = Core.Context.Appointments.Where(a => a.ClientId == user.Id).ToList();
                    Core.Context.Appointments.RemoveRange(userAppointments);

                    var userOrders = Core.Context.Orders.Where(o => o.ClientId == user.Id).ToList();

                    foreach (var order in userOrders)
                    {
                        var orderItems = Core.Context.OrderItems.Where(oi => oi.OrderId == order.Id).ToList();
                        Core.Context.OrderItems.RemoveRange(orderItems);
                    }

                    Core.Context.Orders.RemoveRange(userOrders);

                    Core.Context.Users.Remove(user);

                    Core.Context.SaveChanges();
                    LoadUsers();

                    MessageBox.Show("Пользователь удален");
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}



