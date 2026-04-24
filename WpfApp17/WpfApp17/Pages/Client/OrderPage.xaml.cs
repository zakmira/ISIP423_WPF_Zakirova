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
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        public OrderPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            PaymentMethod.ItemsSource = Core.Context.PaymentMethods.ToList();
            PaymentMethod.SelectedIndex = 0;

            PickupDate.SelectedDate = DateTime.Today.AddDays(1);
            PickupDate.DisplayDateStart = DateTime.Today.AddDays(1);
            PickupDate.DisplayDateEnd = DateTime.Today.AddDays(7);

            var products = Core.Context.Products.ToList();
            var orderItems = Core.CartItems.Select(item => new
            {
                item.ProductId,
                item.Quantity,
                item.PriceAtOrder,
                ProductName = products.FirstOrDefault(p => p.Id == item.ProductId)?.Name ?? "Неизвестно",
                DiscountPercent = products.FirstOrDefault(p => p.Id == item.ProductId)?.DiscountPercent ?? 0,
                TotalPrice = item.PriceAtOrder * item.Quantity * (1 - (decimal)(products.FirstOrDefault(p => p.Id == item.ProductId)?.DiscountPercent ?? 0) / 100)
            }).ToList();

            OrderList.ItemsSource = orderItems;

            decimal total = orderItems.Sum(x => x.TotalPrice);
            TotalAmount.Text = $"{total:F2} ₽";
        }

        private void PaymentMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (PickupDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату получения");
                return;
            }

            if (PaymentMethod.SelectedItem == null)
            {
                MessageBox.Show("Выберите способ оплаты");
                return;
            }

            if (Core.CartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            var result = MessageBox.Show("Подтверждаете заказ?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var products = Core.Context.Products.ToList();
                decimal totalAmount = Core.CartItems.Sum(item =>
                    item.PriceAtOrder * item.Quantity * (1 - (decimal)(products.FirstOrDefault(p => p.Id == item.ProductId)?.DiscountPercent ?? 0) / 100));

                var order = new Orders
                {
                    ClientId = Core.CurrentUserId,
                    OrderDate = DateTime.Now,
                    DesiredPickupDate = PickupDate.SelectedDate.Value,
                    PaymentMethodId = ((PaymentMethods)PaymentMethod.SelectedItem).Id,
                    Status = "New",
                    TotalAmount = totalAmount
                };

                Core.Context.Orders.Add(order);
                Core.Context.SaveChanges();

                foreach (var cartItem in Core.CartItems)
                {
                    var orderItem = new OrderItems
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceAtOrder = cartItem.PriceAtOrder
                    };
                    Core.Context.OrderItems.Add(orderItem);
                }

                Core.Context.SaveChanges();

                Core.CartItems.Clear();

                MessageBox.Show($"Заказ {order.Id} оформлен!");

                NavigationService.Navigate(new ProductsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}");
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