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
    /// Логика взаимодействия для ProductDetails.xaml
    /// </summary>
    public partial class ProductDetails : Page
    {
        private int _productId;

        public ProductDetails(int productId)
        {
            InitializeComponent();
            _productId = productId;
            LoadProductData();
        }

        private void LoadProductData()
        {
            var product = Core.Context.Products.FirstOrDefault(p => p.Id == _productId);

            if (product != null)
            {
                ProductName.Text = product.Name;
                Price.Text = product.Price.ToString("F2") + " ₽";
                Description.Text = product.Description ?? "Описание отсутствует";

                if (product.DiscountPercent > 0)
                {
                    Discount.Text = $"Скидка {product.DiscountPercent}%";

                    decimal discountedPrice = product.Price * (1 - (decimal)product.DiscountPercent / 100);
                    Price.Text = $"{discountedPrice:F2} ₽ (было {product.Price:F2} ₽)";
                }
                else
                {
                    Discount.Text = "Без скидки";
                }

                Rating.Text = product.Rating.ToString("F1");

                if (product.Manufacturers != null)
                {
                    Manufacturer.Text = product.Manufacturers.Name;
                }
                else
                {
                    Manufacturer.Text = "Не указан";
                }

                if (product.ProductTypes != null)
                {
                    ProductType.Text = product.ProductTypes.Name;
                }
                else
                {
                    ProductType.Text = "Не указан";
                }
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                MessageBox.Show("Войдите в аккаунт, чтобы добавить товар в корзину");
                return;
            }

            MessageBox.Show("Товар добавлен в корзину");
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