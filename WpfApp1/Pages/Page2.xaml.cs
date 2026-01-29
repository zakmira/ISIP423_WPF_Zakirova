using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1;

namespace WpfApp1.Pages
{
    public partial class Page2 : Page
    {
        private List<Product> Cart { get; set; }

        public Page2(List<Product> cart)
        {
            InitializeComponent();
            Cart = cart ?? new List<Product>();
            CartList.ItemsSource = Cart;

            UpdateTotalAmount();
            NextPageButton.Click += NextPageButton_Click;
        }

        private void UpdateTotalAmount()
        {
            decimal? total = Cart.Sum(p => p.Price);
            TotalAmountText.Text = $"Итого: {total:C}";
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            var page3 = new Page3(Cart);
            NavigationService?.Navigate(page3);
        }
    }
}





