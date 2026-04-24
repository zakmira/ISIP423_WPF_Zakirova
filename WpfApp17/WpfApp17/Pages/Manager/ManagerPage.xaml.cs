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
using WpfApp17.Pages.Client;

namespace WpfApp17.Pages.Manager
{
    /// <summary>
    /// Логика взаимодействия для Manager.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        public ManagerPage()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            LoadAppointments();
            LoadOrders();
            LoadProducts();
            LoadManufacturers();
            LoadProductTypes();
            LoadServiceTypes();
        }

        private void LoadAppointments()
        {
            var appointments = (from a in Core.Context.Appointments
                                join s in Core.Context.Services on a.ServiceId equals s.Id
                                join client in Core.Context.Users on a.ClientId equals client.Id
                                join master in Core.Context.Users on s.MasterUserId equals master.Id
                                select new
                                {
                                    a.Id,
                                    a.AppointmentDateTime,
                                    a.Status,
                                    ServiceName = s.Name,
                                    FullName = client.FullName,
                                    MasterName = master.FullName,
                                    ClientPhone = client.Phone
                                })
                                .OrderByDescending(a => a.AppointmentDateTime)
                                .ToList();

            string searchText = ClientSearch.Text;
            if (!string.IsNullOrEmpty(searchText))
            {
                appointments = appointments.Where(a =>
                    a.FullName.ToLower().Contains(searchText.ToLower()) ||
                    a.ClientPhone.Contains(searchText)).ToList();
            }

            AppointmentsList.ItemsSource = appointments;
        }

        private void ClientSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadAppointments();
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            ClientSearch.Text = "";
        }

        private void CreateAppointment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Pages.Manager.ManagerCreateAppoint());
        }

        private void Reschedule_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var appointment = button?.Tag;

            if (appointment != null)
            {
                int appointmentId = (int)appointment.GetType().GetProperty("Id").GetValue(appointment);

                var freshAppointment = Core.Context.Appointments.FirstOrDefault(a => a.Id == appointmentId);

                if (freshAppointment == null)
                {
                    MessageBox.Show("Запись не найдена");
                    return;
                }

                if (freshAppointment.Status == "Cancelled")
                {
                    MessageBox.Show("Нельзя перенести отмененную запись");
                    return;
                }

                if (freshAppointment.Status == "Completed")
                {
                    MessageBox.Show("Нельзя перенести выполненную запись");
                    return;
                }

                NavigationService.Navigate(new ManagerReschedule(appointmentId));
            }
        }

        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var appointment = button?.Tag;

            if (appointment != null)
            {
                int appointmentId = (int)appointment.GetType().GetProperty("Id").GetValue(appointment);
                string clientName = (string)appointment.GetType().GetProperty("FullName").GetValue(appointment);

                var freshAppointment = Core.Context.Appointments.FirstOrDefault(a => a.Id == appointmentId);

                if (freshAppointment == null)
                {
                    MessageBox.Show("Запись не найдена");
                    return;
                }

                if (freshAppointment.Status == "Cancelled")
                {
                    MessageBox.Show("Запись уже отменена");
                    return;
                }

                if (freshAppointment.Status == "Completed")
                {
                    MessageBox.Show("Нельзя отменить выполненную запись");
                    return;
                }

                var result = MessageBox.Show($"Отменить запись клиента {clientName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    freshAppointment.Status = "Cancelled";
                    Core.Context.SaveChanges();
                    LoadAppointments();
                    MessageBox.Show("Запись отменена");
                }
            }
        }

        private void LoadProducts()
        {
            var products = Core.Context.Products.ToList();
            ProductsList.ItemsSource = products;
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Manager.ProductEdit(null, () => LoadProducts()));
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.Tag as Products;

            if (product != null)
            {
                NavigationService.Navigate(new Manager.ProductEdit(product, () => LoadProducts()));
            }
        }

        private void Discount_LostFocus(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;
            var product = box?.Tag as Products;

            if (product != null && int.TryParse(box.Text, out int discount))
            {
                if (discount < 0) discount = 0;
                if (discount > 100) discount = 100;

                var productToUpdate = Core.Context.Products.FirstOrDefault(p => p.Id == product.Id);
                if (productToUpdate != null)
                {
                    productToUpdate.DiscountPercent = discount;
                    Core.Context.SaveChanges();
                    LoadProducts();
                }
            }
        }

        private void FreezeProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.Tag as Products;

            if (product != null)
            {
                var productToUpdate = Core.Context.Products.FirstOrDefault(p => p.Id == product.Id);
                if (productToUpdate != null)
                {
                    productToUpdate.IsFrozen = !productToUpdate.IsFrozen;
                    Core.Context.SaveChanges();
                    LoadProducts();
                    MessageBox.Show(productToUpdate.IsFrozen ? "Товар заморожен" : "Товар разморожен");
                }
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.Tag as Products;

            if (product != null)
            {
                var result = MessageBox.Show($"Удалить товар \"{product.Name}\"?\n\nЕсли товар есть в заказах, он будет заморожен (не будет отображаться в каталоге).",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var orderItems = Core.Context.OrderItems.Where(oi => oi.ProductId == product.Id).ToList();

                    if (orderItems.Count > 0)
                    {
                        product.IsFrozen = true;
                        MessageBox.Show($"Товар \"{product.Name}\" заморожен (находится в заказе {orderItems.Count})");
                    }
                    else
                    {
                        Core.Context.Products.Remove(product);
                        MessageBox.Show("Товар удален");
                    }

                    Core.Context.SaveChanges();
                    LoadProducts();
                }
            }
        }

        private void LoadOrders()
        {
            var orders = Core.Context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            if (OrdersList != null)
            {
                OrdersList.ItemsSource = orders;
            }
        }

        private void OrderStatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadOrders();
        }

        private void CompleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button?.Tag as Orders;

            if (order != null && order.Status == "New")
            {
                var result = MessageBox.Show($"Выдать заказ №{order.Id} клиенту {order.Users?.FullName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    order.Status = "Completed"; 
                    Core.Context.SaveChanges();
                    LoadOrders();
                    MessageBox.Show("Заказ выдан");
                }
            }
            else if (order != null && order.Status != "New")
            {
                MessageBox.Show("Можно выдать только новый заказ");
            }
        }

        private void LoadManufacturers()
        {
            ManufacturersList.ItemsSource = Core.Context.Manufacturers.ToList();
        }

        private void AddManufacturer_Click(object sender, RoutedEventArgs e)
        {
            var window = new Pages.Manager.ManufacturerEdit(null);
            if (window.ShowDialog() == true)
            {
                LoadManufacturers();
                MessageBox.Show("Производитель добавлен");
            }
        }

        private void EditManufacturer_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var manufacturer = button?.Tag as Manufacturers;

            if (manufacturer != null)
            {
                var window = new Pages.Manager.ManufacturerEdit(manufacturer);
                if (window.ShowDialog() == true)
                {
                    LoadManufacturers();
                    MessageBox.Show("Производитель обновлен");
                }
            }
        }

        private void DeleteManufacturer_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var manufacturer = button?.Tag as Manufacturers;

            if (manufacturer != null)
            {
                var productsWithThisManufacturer = Core.Context.Products.Where(p => p.ManufacturerId == manufacturer.Id).ToList();

                if (productsWithThisManufacturer.Count > 0)
                {
                    MessageBox.Show($"Нельзя удалить производителя \"{manufacturer.Name}\", так как он используется");
                    return;
                }

                var result = MessageBox.Show($"Удалить производителя \"{manufacturer.Name}\"?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Core.Context.Manufacturers.Remove(manufacturer);
                    Core.Context.SaveChanges();
                    LoadManufacturers();
                }
            }
        }

        private void LoadProductTypes()
        {
            ProductTypesList.ItemsSource = Core.Context.ProductTypes.ToList();
        }

        private void AddProductType_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Manager.ProductTypeEdit(null, () => LoadProductTypes()));
        }

        private void EditProductType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var productType = button?.Tag as ProductTypes;

            if (productType != null)
            {
                NavigationService.Navigate(new Manager.ProductTypeEdit(productType, () => LoadProductTypes()));
            }
        }

        private void DeleteProductType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var productType = button?.Tag as ProductTypes;

            if (productType != null)
            {
                var productsWithThisType = Core.Context.Products.Where(p => p.ProductTypeId == productType.Id).ToList();

                if (productsWithThisType.Count > 0)
                {
                    MessageBox.Show($"Нельзя удалить тип \"{productType.Name}\", так как он используется");
                    return;
                }

                var result = MessageBox.Show($"Удалить тип \"{productType.Name}\"?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Core.Context.ProductTypes.Remove(productType);
                    Core.Context.SaveChanges();
                    LoadProductTypes();
                }
            }
        }

        private void LoadServiceTypes()
        {
            ServiceTypesList.ItemsSource = Core.Context.ServiceTypes.ToList();
        }

        private void AddServiceType_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Manager.ServiceTypeEdit(null, () => LoadServiceTypes()));
        }

        private void EditServiceType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var serviceType = button?.Tag as ServiceTypes;

            if (serviceType != null)
            {
                NavigationService.Navigate(new Manager.ServiceTypeEdit(serviceType, () => LoadServiceTypes()));
            }
        }

        private void DeleteServiceType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var serviceType = button?.Tag as ServiceTypes;

            if (serviceType != null)
            {
                var servicesWithThisType = Core.Context.Services.Where(s => s.ServiceTypeId == serviceType.Id).ToList();

                if (servicesWithThisType.Count > 0)
                {
                    MessageBox.Show($"Нельзя удалить тип услуги \"{serviceType.Name}\", так как он используется");
                    return;
                }

                var result = MessageBox.Show($"Удалить тип услуги \"{serviceType.Name}\"?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Core.Context.ServiceTypes.Remove(serviceType);
                    Core.Context.SaveChanges();
                    LoadServiceTypes();
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
