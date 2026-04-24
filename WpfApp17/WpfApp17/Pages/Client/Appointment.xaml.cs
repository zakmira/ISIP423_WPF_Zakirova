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

namespace WpfApp17.Pages.Client
{
    /// <summary>
    /// Логика взаимодействия для Appointment.xaml
    /// </summary>
    public partial class Appointment : Page
    {
        private int _masterUserId;
        private int _serviceId;

        public Appointment(int masterUserId, int serviceId)
        {
            InitializeComponent();
            _masterUserId = masterUserId;
            _serviceId = serviceId;

            LoadData();
            LoadAvailableTimes();
        }

        private void LoadData()
        {
            var service = Core.Context.Services.FirstOrDefault(s => s.Id == _serviceId);
            var master = Core.Context.Users.FirstOrDefault(u => u.Id == _masterUserId);

            if (service != null)
            {
                Service.Text = service.Name;
                Price.Text = $"{service.Price:C}";
            }

            if (master != null)
            {
                Master.Text = master.FullName;
            }

            Date.SelectedDate = DateTime.Today.AddDays(1);
            Date.DisplayDateStart = DateTime.Today;
            Date.DisplayDateEnd = DateTime.Today.AddMonths(1);

            Payment.ItemsSource = Core.Context.PaymentMethods.ToList();
            if (Payment.Items.Count > 0)
            {
                Payment.SelectedIndex = 0;
            }
        }

        private void LoadAvailableTimes()
        {
            Time.Items.Clear();
            for (int hour = 9; hour <= 20; hour++)
            {
                Time.Items.Add($"{hour:D2}:00");
            }
            Time.SelectedIndex = 0;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (Date.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату");
                return;
            }

            if (Time.SelectedItem == null)
            {
                MessageBox.Show("Выберите время");
                return;
            }

            if (Payment.SelectedItem == null)
            {
                MessageBox.Show("Выберите способ оплаты");
                return;
            }

            if (Core.CurrentUserId == 0)
            {
                MessageBox.Show("Вы не авторизованы");
                return;
            }

            var result = MessageBox.Show("Подтверждаете запись?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            DateTime selectedDate = Date.SelectedDate.Value;
            string timeStr = Time.SelectedItem.ToString();
            int hour = int.Parse(timeStr.Split(':')[0]);
            DateTime appointmentDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, 0, 0);

            if (appointmentDateTime < DateTime.Now)
            {
                MessageBox.Show("Нельзя записаться на прошедшую дату");
                return;
            }

            var appointment = new Appointments
            {
                ClientId = Core.CurrentUserId,
                ServiceId = _serviceId,
                AppointmentDateTime = appointmentDateTime,
                PaymentMethodId = ((PaymentMethods)Payment.SelectedItem).Id,
                Comment = Comment.Text ?? "",
                Status = "Scheduled",
                CreatedAt = DateTime.Now
            };

            Core.Context.Appointments.Add(appointment);
            Core.Context.SaveChanges();

            MessageBox.Show("Вы записаны!");

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