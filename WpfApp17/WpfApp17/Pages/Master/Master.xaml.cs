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

namespace WpfApp17.Pages.Master
{
    /// <summary>
    /// Логика взаимодействия для Master.xaml
    /// </summary>
    public partial class Master : Page
    {
        private int _masterUserId;
        private int _masterId;

        public Master()
        {
            InitializeComponent();
            Loaded += Master_Loaded;
        }

        private void Master_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            var user = Core.Context.Users.FirstOrDefault(u => u.Id == Core.CurrentUserId);
            if (user != null)
            {
                Welcome.Text = $"Добро пожаловать, {user.FullName}";
            }

            var master = Core.Context.Masters.FirstOrDefault(m => m.UserId == Core.CurrentUserId);
            if (master != null)
            {
                _masterUserId = master.UserId;
                _masterId = master.Id;
            }

            LoadAppointments();
            LoadServices();
        }

        private void LoadAppointments()
        {
            var appointments = (from a in Core.Context.Appointments
                                join s in Core.Context.Services on a.ServiceId equals s.Id
                                join u in Core.Context.Users on a.ClientId equals u.Id
                                where s.MasterUserId == Core.CurrentUserId
                                orderby a.AppointmentDateTime descending
                                select new
                                {
                                    a.Id,
                                    a.AppointmentDateTime,
                                    a.Status,
                                    ServiceName = s.Name,
                                    ClientName = u.FullName,
                                    ClientPhone = u.Phone,
                                    Comment = a.Comment
                                }).ToList();

            Appointments.ItemsSource = appointments;
        }

        private void LoadServices()
        {
            var services = Core.Context.Services
                .Where(s => s.MasterUserId == Core.CurrentUserId && s.IsActive == true)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Price,
                    ServiceTypeName = s.ServiceTypes != null ? s.ServiceTypes.Name : "Без категории"
                })
                .ToList();

            Services.ItemsSource = services;
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                LoadAppointments();
            }
        }

        private void Appointment_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            dynamic appointment = border?.DataContext;

            if (appointment != null)
            {
                NavigationService.Navigate(new AppointmentDetails(appointment.Id));
            }
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MasterAddService(_masterId));
        }

        private void RemoveService_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var service = button?.Tag;

            if (service != null)
            {
                int serviceId = (int)service.GetType().GetProperty("Id").GetValue(service);
                string serviceName = (string)service.GetType().GetProperty("Name").GetValue(service);

                var result = MessageBox.Show($"Удалить услугу \"{serviceName}\"?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var serviceToRemove = Core.Context.Services.FirstOrDefault(s => s.Id == serviceId);
                    if (serviceToRemove != null)
                    {
                        serviceToRemove.IsActive = false;
                        Core.Context.SaveChanges();
                        LoadServices();

                        MessageBox.Show("Услуга удалена");
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
