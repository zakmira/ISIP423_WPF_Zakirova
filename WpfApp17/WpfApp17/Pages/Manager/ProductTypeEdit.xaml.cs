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
    /// Логика взаимодействия для ProductTypeEdit.xaml
    /// </summary>
    public partial class ProductTypeEdit : Page
    {
        private ProductTypes _productType;
        private Action _onSaved;

        public ProductTypeEdit(ProductTypes productType, Action onSaved = null)
        {
            InitializeComponent();
            _onSaved = onSaved;

            if (productType != null)
            {
                _productType = productType;
                Name.Text = productType.Name;
            }
            else
            {
                _productType = new ProductTypes();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                MessageBox.Show("Введите название типа товара");
                Name.Focus();
                return;
            }

            _productType.Name = Name.Text.Trim();

            if (_productType.Id == 0)
            {
                Core.Context.ProductTypes.Add(_productType);
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
