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

    public partial class Page2 : Page
    {
        private CarConfig CurrentConfig;
        public string SelectedColor { get; private set; } = "Белый"; // значение по умолчанию
        public string SelectedOption { get; private set; } = "Мультимедиа";

        public Page2(CarConfig config)
        {
            InitializeComponent();

            if (config == null)
            {
                MessageBox.Show("Ошибка: config == null!");
                return;
            }

            CurrentConfig = config;
        }

        private void ColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (CurrentConfig == null)
            {
                MessageBox.Show("Ошибка: CurrentConfig не инициализирован!");
                return;
            }

            if (sender is RadioButton rb && rb.IsChecked == true)
            {
                CurrentConfig.SelectedColor = rb.Content.ToString();

                switch (CurrentConfig.SelectedColor)
                {
                    case "Белый":
                        CurrentConfig.ColorPrice = 0;
                        break;
                    case "Черный":
                        CurrentConfig.ColorPrice = 20000;
                        break;
                    case "Красный":
                        CurrentConfig.ColorPrice = 30000;
                        break;
                    case "Синий":
                        CurrentConfig.ColorPrice = 50000;
                        break;
                }
            }
        }

        private void OptionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (CurrentConfig == null)
            {
                MessageBox.Show("Ошибка: CurrentConfig не инициализирован!");
                return;
            }

            if (sender is RadioButton rb && rb.IsChecked == true)
            {
                CurrentConfig.SelectedOption = rb.Content.ToString();

                switch (CurrentConfig.SelectedOption)
                {
                    case "Мультимедиа":
                        CurrentConfig.OptionPrice = 30000;
                        break;
                    case "Антикоррозия":
                        CurrentConfig.OptionPrice = 20000;
                        break;
                    case "Климат-контроль":
                        CurrentConfig.OptionPrice = 25000;
                        break;
                }
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            var page3 = new Page3(CurrentConfig);
            this.NavigationService.Navigate(page3);
        }
    }
}
