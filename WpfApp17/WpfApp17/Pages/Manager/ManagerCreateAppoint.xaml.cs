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
    /// Логика взаимодействия для ManagerCreayeAppoint.xaml
    /// </summary>
    public partial class ManagerCreateAppoint : Page
    {
        private int _selectedServiceId = 0;

        public ManagerCreateAppoint()
        {
            InitializeComponent();
            LoadServices();
            LoadTimes();
            LoadPayments();
            Date.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void LoadServices()
        {
            var services = Core.Context.Services.Where(s => s.IsActive == true).ToList();
            Service.ItemsSource = services;
        }

        private void LoadTimes()
        {
            Time.Items.Clear();
            for (int hour = 9; hour <= 20; hour++)
            {
                Time.Items.Add($"{hour:D2}:00");
            }
            Time.SelectedIndex = 0;
        }

        private void LoadPayments()
        {
            Payment.ItemsSource = Core.Context.PaymentMethods.ToList();
            if (Payment.Items.Count > 0)
                Payment.SelectedIndex = 0;
        }

        private void Service_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = Service.SelectedItem as Services;
            if (selected != null)
            {
                _selectedServiceId = selected.Id;
                var master = Core.Context.Users.FirstOrDefault(u => u.Id == selected.MasterUserId);
                if (master != null)
                {
                    Master.Text = master.FullName;
                }
            }
        }

        private bool IsPhoneExists(string phone)
        {
            return Core.Context.Users.Any(u => u.Phone == phone);
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            foreach (char c in cleanPhone)
            {
                if (!char.IsDigit(c) && c != '+')
                    return false;
            }

            string digitsOnly = cleanPhone.Replace("+", "");
            return digitsOnly.Length >= 10 && digitsOnly.Length <= 12;
        }

        private string NormalizePhone(string phone)
        {
            string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (cleanPhone.StartsWith("8") && cleanPhone.Length == 11)
            {
                cleanPhone = "+7" + cleanPhone.Substring(1);
            }
            else if (cleanPhone.StartsWith("7") && cleanPhone.Length == 11)
            {
                cleanPhone = "+" + cleanPhone;
            }
            else if (!cleanPhone.StartsWith("+") && cleanPhone.Length == 10)
            {
                cleanPhone = "+7" + cleanPhone;
            }

            return cleanPhone;
        }

        private void ClientPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            string phone = ClientPhone.Text;
            if (!string.IsNullOrEmpty(phone))
            {
                if (!IsValidPhone(phone))
                {
                    PhoneWarning.Visibility = Visibility.Visible;
                }
                else
                {
                    PhoneWarning.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                PhoneWarning.Visibility = Visibility.Collapsed;
            }
        }

        private int CreateNewClient(string name, string phone)
        {
            int clientCount = Core.Context.Users.Count(u => u.Role == "Client");
            int newNumber = clientCount + 1;
            string login = $"client{newNumber}";

            var newClient = new Users
            {
                FullName = name.Trim(),
                Phone = phone.Trim(),
                Login = login,
                Password = "123",
                Role = "Client",
                IsFrozen = false,
                RegistrationDate = DateTime.Now
            };

            Core.Context.Users.Add(newClient);
            Core.Context.SaveChanges();

            return newClient.Id;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClientName.Text))
            {
                MessageBox.Show("Введите имя клиента");
                ClientName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ClientPhone.Text))
            {
                MessageBox.Show("Введите номер телефона клиента");
                ClientPhone.Focus();
                return;
            }

            if (!IsValidPhone(ClientPhone.Text))
            {
                MessageBox.Show("Введите корректный номер телефона");
                ClientPhone.Focus();
                return;
            }

            if (_selectedServiceId == 0)
            {
                MessageBox.Show("Выберите услугу");
                return;
            }

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

            string normalizedPhone = NormalizePhone(ClientPhone.Text);
            int clientId;

            if (IsPhoneExists(normalizedPhone))
            {
                var existingClient = Core.Context.Users.First(u => u.Phone == normalizedPhone);
                clientId = existingClient.Id;

                var result = MessageBox.Show($"Клиент с номером {normalizedPhone} уже существует. Использовать существующего?",
                    "Клиент найден", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }
            else
            {
                clientId = CreateNewClient(ClientName.Text.Trim(), normalizedPhone);
                MessageBox.Show($"Новый клиент: {ClientName.Text}");
            }

            DateTime selectedDate = Date.SelectedDate.Value;
            string timeStr = Time.SelectedItem.ToString();
            int hour = int.Parse(timeStr.Split(':')[0]);
            DateTime appointmentDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, 0, 0);

            if (appointmentDateTime < DateTime.Now)
            {
                MessageBox.Show("Нельзя создать запись на прошедшую дату и время");
                return;
            }

            var service = Core.Context.Services.FirstOrDefault(s => s.Id == _selectedServiceId);
            if (service == null)
            {
                MessageBox.Show("Услуга не найдена");
                return;
            }

            var master = Core.Context.Masters.FirstOrDefault(m => m.UserId == service.MasterUserId);
            if (master == null)
            {
                MessageBox.Show("Мастер не найден");
                return;
            }

            var appointment = new Appointments
            {
                ClientId = clientId,
                ServiceId = _selectedServiceId,
                AppointmentDateTime = appointmentDateTime,
                PaymentMethodId = ((PaymentMethods)Payment.SelectedItem).Id,
                Status = "Scheduled",
                CreatedAt = DateTime.Now
            };

            Core.Context.Appointments.Add(appointment);
            Core.Context.SaveChanges();

            MessageBox.Show("Запись создана!");

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