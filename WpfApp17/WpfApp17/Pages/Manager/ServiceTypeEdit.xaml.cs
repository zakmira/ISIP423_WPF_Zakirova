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
    /// Логика взаимодействия для ServiceTypeEdit.xaml
    /// </summary>
    public partial class ServiceTypeEdit : Page
    {
        private ServiceTypes _serviceType;
        private Action _onSaved;

        public ServiceTypeEdit(ServiceTypes serviceType, Action onSaved = null)
        {
            InitializeComponent();
            _onSaved = onSaved;

            if (serviceType != null)
            {
                _serviceType = serviceType;
                Name.Text = serviceType.Name;
            }
            else
            {
                _serviceType = new ServiceTypes();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                MessageBox.Show("Введите название типа услуги");
                Name.Focus();
                return;
            }

            _serviceType.Name = Name.Text.Trim();

            if (_serviceType.Id == 0)
            {
                Core.Context.ServiceTypes.Add(_serviceType);
            }

            Core.Context.SaveChanges();

            _onSaved?.Invoke();

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
