using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace WpfApp17.Pages.Client
{
    /// <summary>
    /// Логика взаимодействия для Client.xaml
    /// </summary>
    public partial class Client : Page
    {
        public Client()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var user = Core.Context.Users.FirstOrDefault(u => u.Id == Core.CurrentUserId);
            if (user != null)
            {
                Welcome.Text = $"Добро пожаловать, {user.FullName}";
            }

            LoadAppointments();
            LoadOrders();
        }

        private void LoadAppointments()
        {
            var appointments = new List<object>();

            var userAppointments = Core.Context.Appointments
                .Where(a => a.ClientId == Core.CurrentUserId)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToList();

            foreach (var a in userAppointments)
            {
                var service = Core.Context.Services.FirstOrDefault(s => s.Id == a.ServiceId);
                if (service != null)
                {
                    var master = Core.Context.Users.FirstOrDefault(u => u.Id == service.MasterUserId);

                    appointments.Add(new
                    {
                        a.Id,
                        a.AppointmentDateTime,
                        a.Status,
                        ServiceName = service?.Name ?? "Неизвестно",
                        MasterName = master?.FullName ?? "Неизвестно"
                    });
                }
            }

            AppointmentsList.ItemsSource = appointments;
        }

        private void LoadOrders()
        {
            var orders = Core.Context.Orders
                .Where(o => o.ClientId == Core.CurrentUserId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var ordersWithItems = new List<object>();

            foreach (var order in orders)
            {
                var items = new List<string>();
                var orderItems = Core.Context.OrderItems
                    .Where(oi => oi.OrderId == order.Id)
                    .ToList();

                foreach (var item in orderItems)
                {
                    var product = Core.Context.Products
                        .FirstOrDefault(p => p.Id == item.ProductId);

                    if (product != null)
                    {
                        items.Add($"{item.Quantity} x {product.Name} = {item.PriceAtOrder * item.Quantity:F2} ₽");
                    }
                }

                ordersWithItems.Add(new
                {
                    order.Id,
                    order.OrderDate,
                    order.DesiredPickupDate,
                    order.Status,
                    order.TotalAmount,
                    Items = items
                });
            }

            OrdersList.ItemsSource = ordersWithItems;
        }

        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var appointment = button?.Tag;

            if (appointment != null)
            {
                var appointmentId = (int)appointment.GetType().GetProperty("Id").GetValue(appointment);
                var appointmentStatus = (string)appointment.GetType().GetProperty("Status").GetValue(appointment);

                if (appointmentStatus == "Cancelled")
                {
                    MessageBox.Show("Запись уже отменена");
                    return;
                }

                if (appointmentStatus == "Completed")
                {
                    MessageBox.Show("Нельзя отменить выполненную запись");
                    return;
                }

                var result = MessageBox.Show("Отменить запись?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var appointmentToCancel = Core.Context.Appointments
                        .FirstOrDefault(a => a.Id == appointmentId);

                    if (appointmentToCancel != null)
                    {
                        appointmentToCancel.Status = "Cancelled";
                        Core.Context.SaveChanges();
                        LoadAppointments();

                        MessageBox.Show("Запись отменена");
                    }
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
