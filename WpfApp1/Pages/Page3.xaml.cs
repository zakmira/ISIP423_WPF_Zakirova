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
    /// Логика взаимодействия для Page3.xaml
    /// </summary>
    public partial class Page3 : Page
    {
        private CarConfig CurrentConfig;
        public Page3(CarConfig config)
        {
            InitializeComponent();
            CurrentConfig = config;

            ModelTextBlock.Text = $"Модель: {config.SelectedModel} ({config.ModelPrice:C})";
            EngineTextBlock.Text = $"Двигатель: {config.SelectedEngine} ({config.EnginePrice:C})";
            ColorTextBlock.Text = $"Цвет: {config.SelectedColor} ({config.ColorPrice:C})";
            OptionTextBlock.Text = $"Опция: {config.SelectedOption} ({config.OptionPrice:C})";
            TotalTextBlock.Text = $"ИТОГО: {config.TotalPrice:C}";
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentConfig == null)
            {
                MessageBox.Show("Ошибка: CurrentConfig == null!");
                return;
            }

            var page4 = new Page4(CurrentConfig);
            this.NavigationService.Navigate(page4);
        }
    }
}
