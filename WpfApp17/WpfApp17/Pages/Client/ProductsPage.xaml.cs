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
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        public ProductsPage()
        {
            InitializeComponent();
            LoadFilters();
            LoadProducts();
        }

        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Search.Text == "Поиск по названию")
            {
                Search.Text = "";
                Search.Foreground = Brushes.Black;
            }
        }

        private void Search_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Search.Text))
            {
                Search.Text = "Поиск по названию";
                Search.Foreground = Brushes.Gray;
            }
        }

        private void LoadFilters()
        {
            var productTypes = Core.Context.ProductTypes.ToList();
            ProductTypeFilter.ItemsSource = productTypes;
            ProductTypeFilter.SelectedIndex = -1;

            var manufacturers = Core.Context.Manufacturers.ToList();
            ManufacturerFilter.ItemsSource = manufacturers;
            ManufacturerFilter.SelectedIndex = -1;
        }

        private void LoadProducts()
        {
            var products = Core.Context.Products.Where(p => p.IsFrozen == false).ToList();

            string searchText = Search.Text;
            if (!string.IsNullOrEmpty(searchText) && searchText != "Поиск по названию")
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchText.ToLower())).ToList();
            }

            if (ProductTypeFilter.SelectedItem != null && ProductTypeFilter.SelectedItem is ProductTypes)
            {
                var selectedType = (ProductTypes)ProductTypeFilter.SelectedItem;
                products = products.Where(p => p.ProductTypeId == selectedType.Id).ToList();
            }

            if (ManufacturerFilter.SelectedItem != null && ManufacturerFilter.SelectedItem is Manufacturers)
            {
                var selectedManufacturer = (Manufacturers)ManufacturerFilter.SelectedItem;
                products = products.Where(p => p.ManufacturerId == selectedManufacturer.Id).ToList();
            }

            if (Sort.SelectedIndex == 1)
            {
                products = products.OrderByDescending(p => p.Rating).ToList();
            }
            else if (Sort.SelectedIndex == 2)
            {
                products = products.OrderBy(p => p.Rating).ToList();
            }

            if (ProductList != null)
            {
                ProductList.ItemsSource = products;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ProductList is null!");
            }
        }

        private double GetAverageRating(int productId)
        {
            var product = Core.Context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return 0;
            return (double)product.Rating;
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }

        private void Sort_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.Tag as Products;

            if (product != null)
            {
                NavigationService.Navigate(new ProductDetails(product.Id));
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                MessageBox.Show("Войдите в аккаунт, чтобы добавить товар в корзину");
                return;
            }

            var button = sender as Button;
            var product = button?.Tag as Products;

            if (product != null && product.IsFrozen == false)
            {
                var existing = Core.CartItems.FirstOrDefault(x => x.ProductId == product.Id);
                if (existing != null)
                {
                    existing.Quantity++;
                }
                else
                {
                    Core.CartItems.Add(new OrderItems
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        PriceAtOrder = product.Price
                    });
                }

                MessageBox.Show($"Товар \"{product.Name}\" добавлен в корзину");
            }
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                MessageBox.Show("Войдите в аккаунт, чтобы посмотреть корзину");
                return;
            }

            NavigationService.Navigate(new Cart());
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