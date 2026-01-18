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

namespace WpfApp1.Pages
{
    /// <summary>
    /// Логика взаимодействия для Page4.xaml
    /// </summary>
    public partial class Page4 : Page
    {
        private readonly CarConfig CurrentConfig;
        public Page4(CarConfig config)
        {
            InitializeComponent();
            
            if (config == null)
            {
                MessageBox.Show("Ошибка: config == null!");
                return;
            }

            CurrentConfig = config;
            CarPriceText.Text = $"{CurrentConfig.TotalPrice:C}";
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal payment = decimal.Parse(DownPaymentBox.Text);
                decimal percent = decimal.Parse(RateBox.Text);
                int months = int.Parse(TermBox.Text);

                if (payment < 0 || payment > 1000000)
                {
                    ResultText.Text = "Первоначальный взнос должен быть от нуля до миллиона!";
                    return;
                }

                if (percent <= 0)
                {
                    ResultText.Text = "Ставка должна быть больше нуля!";
                    return;
                }

                if (months <= 1)
                {
                    ResultText.Text = "Срок должен быть больше одного месяца!";
                    return;
                }

                decimal carPrice = CurrentConfig.TotalPrice;
                decimal creditAmount = carPrice - payment; // S = C - P

                // i = r / 100 / 12
                double monthlyRate = (double)(percent / 100 / 12);

                // A = S * (i * (1+i)^n) / ((1+i)^(n - 1)
                double i = monthlyRate;
                double n = months;
                double numerator = i * Math.Pow(1 + i, n);
                double denominator = Math.Pow(1 + i, n - 1) ;

                if (denominator <= 0)
                {
                    ResultText.Text = "Ошибка: невозможно рассчитать платёж";
                    return;
                }

                double monthlyPayment = (double)creditAmount * (numerator / denominator);
                decimal totalOverpayment = (decimal)monthlyPayment * months - creditAmount;

                // 3. Выводим результат
                ResultText.Text = $"Ежемесячный платёж: {monthlyPayment:C}\n" +
                                  $"Общая переплата: {totalOverpayment:C}";

            }
            catch (Exception ex) when(ex is FormatException || ex is OverflowException)
            {
            ResultText.Text = "Введите корректные числа!";
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentConfig == null)
            {
                MessageBox.Show("Ошибка: CurrentConfig == null!");
                return;
            }

            var page5 = new Page5(CurrentConfig);
            this.NavigationService.Navigate(page5);
        }
    }         
}
