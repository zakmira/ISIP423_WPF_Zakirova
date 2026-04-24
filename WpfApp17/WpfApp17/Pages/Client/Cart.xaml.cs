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
    /// Логика взаимодействия для Cart.xaml
    /// </summary>
    public partial class Cart : Page
    {
        public Cart()
        {
            InitializeComponent();
            LoadCart();
        }

        private void LoadCart()
        {
            if (Core.CartItems.Count == 0)
            {
                CartList.Visibility = Visibility.Collapsed;
                EmptyCartPanel.Visibility = Visibility.Visible;
                TotalAmount.Text = "0 ₽";
            }
            else
            {
                CartList.Visibility = Visibility.Visible;
                EmptyCartPanel.Visibility = Visibility.Collapsed;

                var products = Core.Context.Products.ToList();
                var cartWithNames = Core.CartItems.Select(item => new
                {
                    item.ProductId,
                    item.Quantity,
                    item.PriceAtOrder,
                    ProductName = products.FirstOrDefault(p => p.Id == item.ProductId)?.Name ?? "Неизвестно",
                    DiscountPercent = products.FirstOrDefault(p => p.Id == item.ProductId)?.DiscountPercent ?? 0,
                    TotalPrice = item.PriceAtOrder * item.Quantity * (1 - (decimal)(products.FirstOrDefault(p => p.Id == item.ProductId)?.DiscountPercent ?? 0) / 100)
                }).ToList();

                CartList.ItemsSource = cartWithNames;
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            var products = Core.Context.Products.ToList();
            decimal total = Core.CartItems.Sum(item =>
                item.PriceAtOrder * item.Quantity * (1 - (decimal)(products.FirstOrDefault(p => p.Id == item.ProductId)?.DiscountPercent ?? 0) / 100));
            TotalAmount.Text = $"{total:F2} ₽";
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            dynamic item = button?.Tag;
            int productId = item.ProductId;

            var cartItem = Core.CartItems.FirstOrDefault(x => x.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity++;
                LoadCart();
            }
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            dynamic item = button?.Tag;
            int productId = item.ProductId;

            var cartItem = Core.CartItems.FirstOrDefault(x => x.ProductId == productId);
            if (cartItem != null && cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                LoadCart();
            }
            else if (cartItem != null && cartItem.Quantity == 1)
            {
                Core.CartItems.Remove(cartItem);
                LoadCart();
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            dynamic item = button?.Tag;
            int productId = item.ProductId;

            var cartItem = Core.CartItems.FirstOrDefault(x => x.ProductId == productId);
            if (cartItem != null)
            {
                Core.CartItems.Remove(cartItem);
                LoadCart();
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                MessageBox.Show("Войдите в аккаунт для оформления заказа");
                return;
            }

            if (Core.CartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainFrame.Navigate(new Pages.Client.OrderPage());
            }
        }

        private void GoToProducts_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainFrame.Navigate(new ProductsPage());
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null && mainWindow.MainFrame.CanGoBack)
            {
                mainWindow.MainFrame.GoBack();
            }
        }
    }
}