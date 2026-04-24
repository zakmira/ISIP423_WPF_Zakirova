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
    /// Логика взаимодействия для ProductEdit.xaml
    /// </summary>
    public partial class ProductEdit : Page
    {
        private Products _product;
        private Action _onSaved;

        public ProductEdit(Products product, Action onSaved = null)
        {
            InitializeComponent();
            _onSaved = onSaved;
            LoadComboBoxes();

            if (product != null)
            {
                _product = product;
                LoadProductData();
            }
            else
            {
                _product = new Products();
            }
        }

        private void LoadComboBoxes()
        {
            Manufacturer.ItemsSource = Core.Context.Manufacturers.ToList();
            ProductType.ItemsSource = Core.Context.ProductTypes.ToList();
        }

        private void LoadProductData()
        {
            Name.Text = _product.Name;
            Price.Text = _product.Price.ToString();
            Description.Text = _product.Description;
            Discount.Text = _product.DiscountPercent.ToString();

            Manufacturer.SelectedItem = Manufacturer.Items
                .Cast<Manufacturers>()
                .FirstOrDefault(m => m.Id == _product.ManufacturerId);

            ProductType.SelectedItem = ProductType.Items
                .Cast<ProductTypes>()
                .FirstOrDefault(t => t.Id == _product.ProductTypeId);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                MessageBox.Show("Введите название товара");
                Name.Focus();
                return;
            }

            if (!decimal.TryParse(Price.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену");
                Price.Focus();
                return;
            }

            if (!int.TryParse(Discount.Text, out int discount))
            {
                discount = 0;
            }
            if (discount < 0) discount = 0;
            if (discount > 100) discount = 100;

            if (Manufacturer.SelectedItem == null)
            {
                MessageBox.Show("Выберите производителя");
                return;
            }

            if (ProductType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип товара");
                return;
            }

            _product.Name = Name.Text.Trim();
            _product.Price = price;
            _product.Description = string.IsNullOrWhiteSpace(Description.Text) ? null : Description.Text.Trim();
            _product.DiscountPercent = discount;
            _product.ManufacturerId = ((Manufacturers)Manufacturer.SelectedItem).Id;
            _product.ProductTypeId = ((ProductTypes)ProductType.SelectedItem).Id;

            if (_product.Id == 0)
            {
                _product.IsFrozen = false;
                Core.Context.Products.Add(_product);
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
