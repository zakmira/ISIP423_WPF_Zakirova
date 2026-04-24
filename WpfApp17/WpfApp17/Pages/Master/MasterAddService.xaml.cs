using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для MasterAddService.xaml
    /// </summary>
    public partial class MasterAddService : Page
    {
        private int _masterId;

        public MasterAddService(int masterId)
        {
            InitializeComponent();
            _masterId = masterId;
            LoadServiceTypes();
        }

        private void LoadServiceTypes()
        {
            var types = Core.Context.ServiceTypes.ToList();
            ServiceType.ItemsSource = types;
            if (ServiceType.Items.Count > 0)
            {
                ServiceType.SelectedIndex = 0;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServiceName.Text))
            {
                MessageBox.Show("Введите название услуги");
                return;
            }

            if (!decimal.TryParse(Price.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную стоимость");
                return;
            }

            if (!int.TryParse(Duration.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Введите корректную длительность");
                return;
            }

            if (ServiceType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип услуги");
                return;
            }

            var master = Core.Context.Masters.FirstOrDefault(m => m.Id == _masterId);
            if (master == null)
            {
                MessageBox.Show("Мастер не найден");
                return;
            }

            int serviceTypeId = ((ServiceTypes)ServiceType.SelectedItem).Id;

            var newService = new Services
            {
                Name = ServiceName.Text.Trim(),
                Price = price,
                DurationMinutes = duration,
                ServiceTypeId = serviceTypeId,
                MasterUserId = master.UserId,
                IsActive = true
            };

            Core.Context.Services.Add(newService);
            Core.Context.SaveChanges();

            MessageBox.Show("Услуга успешно добавлена");

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
