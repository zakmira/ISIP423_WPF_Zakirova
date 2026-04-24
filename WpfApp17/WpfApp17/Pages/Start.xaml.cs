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

namespace WpfApp17.Pages
{
    /// <summary>
    /// Логика взаимодействия для Start.xaml
    /// </summary>
    public partial class Start : Page
    {
        private int? _selectedMasterId = null;
        private int? _selectedServiceId = null;

        public Start()
        {
            InitializeComponent();
            LoadFilters();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilters();
        }

        private void LoadFilters()
        {
            var masters = Core.Context.Users
                .Where(u => u.Role == "Master" && u.IsFrozen == false)
                .ToList();
            Master.ItemsSource = masters;

            var services = Core.Context.Services.Where(s => s.IsActive == true).ToList();
            Service.ItemsSource = services;
        }

        private void Service_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Service.SelectedItem == null) return;
            
            var selectedService = (Services)Service.SelectedItem;
            _selectedServiceId = selectedService.Id;
            
            var master = Core.Context.Users.FirstOrDefault(u => u.Id == selectedService.MasterUserId);
            
            if (master != null)
            {
                var masterList = new List<Users> { master };
                AvailableMasters.ItemsSource = masterList;
            }
        }

        private void Master_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Master.SelectedItem == null) return;

            dynamic selectedMaster = Master.SelectedItem;
            _selectedMasterId = selectedMaster.Id;

            var masterServices = Core.Context.Services
                .Where(s => s.MasterUserId == _selectedMasterId && s.IsActive == true)
                .ToList();

            Service.SelectedItem = null;
            Service.ItemsSource = masterServices;
        }

        private void AvailableMasters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Book.IsEnabled = AvailableMasters.SelectedItem != null;
        }

        private void Book_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                MessageBox.Show("Для записи войдите в аккаунт");
                return;
            }

            var selectedMaster = (Users)AvailableMasters.SelectedItem;
            var selectedService = (Services)Service.SelectedItem;

            if (selectedMaster != null && selectedService != null)
            {
                if (Core.CurrentUserId == selectedMaster.Id)
                {
                    MessageBox.Show("Мастер не может записаться на услугу к самому себе");
                    return;
                }

                int masterId = selectedMaster.Id;
                int serviceId = selectedService.Id;

                NavigationService.Navigate(new Pages.Client.Appointment(masterId, serviceId));
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
