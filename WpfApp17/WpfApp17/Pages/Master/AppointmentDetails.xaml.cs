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
    /// Логика взаимодействия для AppointmentDetails.xaml
    /// </summary>
    public partial class AppointmentDetails : Page
    {
        private int _appointmentId;

        public AppointmentDetails(int appointmentId)
        {
            InitializeComponent();
            _appointmentId = appointmentId;
            LoadData();
        }

        private void LoadData()
        {
            var appointment = Core.Context.Appointments.FirstOrDefault(a => a.Id == _appointmentId);

            if (appointment != null)
            {
                var service = Core.Context.Services.FirstOrDefault(s => s.Id == appointment.ServiceId);
                var client = Core.Context.Users.FirstOrDefault(u => u.Id == appointment.ClientId);

                Service.Text = service?.Name ?? "Неизвестно";
                DateTime.Text = appointment.AppointmentDateTime.ToString("dd.MM.yyyy HH:mm");
                Client.Text = client?.FullName ?? "Неизвестно";
                Phone.Text = client?.Phone ?? "Неизвестно";
                Comment.Text = string.IsNullOrEmpty(appointment.Comment) ? "Нет комментария" : appointment.Comment;

                if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
                {
                    Complete.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Complete.Visibility = Visibility.Visible;
                }
            }
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            var appointment = Core.Context.Appointments.FirstOrDefault(a => a.Id == _appointmentId);

            if (appointment != null)
            {
                if (appointment.Status == "Cancelled")
                {
                    MessageBox.Show("Нельзя завершить отмененную запись");
                    return;
                }

                if (appointment.Status == "Completed")
                {
                    MessageBox.Show("Запись уже выполнена");
                    return;
                }

                var result = MessageBox.Show("Подтвердите, что услуга выполнена", "Завершение записи",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    appointment.Status = "Completed";
                    Core.Context.SaveChanges();

                    MessageBox.Show("Запись отмечена как выполненная");

                    Complete.Visibility = Visibility.Collapsed;

                    if (NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
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