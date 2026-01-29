using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace WpfApp1.Pages
{
    public partial class Page1 : Page
    {
        private List<Product> Cart = new List<Product>();

        public Page1()
        {
            InitializeComponent();
            LoadProducts();
            NextPageButton.Click += NextPageButton_Click;
        }

        private void LoadProducts()
        {
            try
            {
                var products = Core.Context.Product.ToList();
                ProductList.ItemsSource = products;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Count == 0)
            {
                MessageBox.Show("Добавьте товары в корзину!");
                return;
            }

            var page2 = new Page2(Cart); 
            NavigationService?.Navigate(new Page2(Cart));
        }

        private void AddToCart(Product product)
        {
            Cart.Add(product);
            MessageBox.Show($"{product.Name} добавлен в корзину!");
        }

        private void ToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.DataContext as Product;
            if (product != null)
            {
                AddToCart(product);
            }
        }

    }
}
