using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class Page3 : Page
    {
        private List<Product> Cart { get; set; }

        public Page3(List<Product> cart)
        {
            InitializeComponent();
            Cart = cart ?? new List<Product>();
            OrderItemsList.ItemsSource = Cart;

            UpdateTotalAmount();
            OrderButton.Click += OrderButton_Click;

        }

        private void UpdateTotalAmount()
        {
            decimal? total = Cart.Sum(p => p.Price);
            TotalAmountText.Text = $"Итого: {total:N2} ₽";
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите ФИО");
                return;
            }

            if (string.IsNullOrWhiteSpace(AddressBox.Text))
            {
                MessageBox.Show("Введите адрес доставки");
                return;
            }

            Order order = new Order
            {
                FIO = NameBox.Text.Trim(),
                Email = MailBox.Text?.Trim(),
                Address = AddressBox.Text.Trim(),

            };

                Core.Context.Order.Add(order);
                Core.Context.SaveChanges(); 

                foreach (var product in Cart)
                {
                    var orderProduct = new OrderProduct
                    {
                        OrderID = order.ID,   
                        ProductID = product.ID,
                    };
                    Core.Context.OrderProduct.Add(orderProduct);
                }

                Core.Context.SaveChanges(); 

                MessageBox.Show("Заказ оформлен!");
                NavigationService?.GoBack();
        }
    }
}