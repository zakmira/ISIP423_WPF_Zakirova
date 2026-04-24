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

namespace WpfApp17.Pages.Manager
{
    /// <summary>
    /// Логика взаимодействия для ManagerReschedule.xaml
    /// </summary>
    public partial class ManagerReschedule : Page
    {
        private int _appointmentId;

        public ManagerReschedule(int appointmentId)
        {
            InitializeComponent();
            _appointmentId = appointmentId;
            LoadAppointmentInfo();
            LoadTimes();
            NewDate.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void LoadAppointmentInfo()
        {
            var appointment = Core.Context.Appointments.FirstOrDefault(a => a.Id == _appointmentId);
            if (appointment != null)
            {
                var service = Core.Context.Services.FirstOrDefault(s => s.Id == appointment.ServiceId);
                var client = Core.Context.Users.FirstOrDefault(u => u.Id == appointment.ClientId);
                AppointmentInfo.Text = $"Клиент: {client?.FullName}\nУслуга: {service?.Name}\nТекущая дата: {appointment.AppointmentDateTime:dd.MM.yyyy HH:mm}";
            }
        }

        private void LoadTimes()
        {
            NewTime.Items.Clear();
            for (int hour = 9; hour <= 20; hour++)
            {
                NewTime.Items.Add($"{hour:D2}:00");
            }
            NewTime.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (NewDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите новую дату");
                return;
            }

            DateTime selectedDate = NewDate.SelectedDate.Value;
            string timeStr = NewTime.SelectedItem.ToString();
            int hour = int.Parse(timeStr.Split(':')[0]);
            DateTime newDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, 0, 0);

            var appointment = Core.Context.Appointments.FirstOrDefault(a => a.Id == _appointmentId);
            if (appointment != null)
            {
                appointment.AppointmentDateTime = newDateTime;
                appointment.Status = "Scheduled";
                Core.Context.SaveChanges();
                MessageBox.Show("Запись перенесена");
            }

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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
