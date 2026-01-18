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
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        private CarConfig CurrentConfig = new CarConfig();
        public string SelectedModel { get; private set; } = "Toyota Camry"; // значение по умолчанию
        public string SelectedEngine { get; private set; } = "ДВС";

        public Page1()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine($"CurrentConfig в Page1: {CurrentConfig}");
        }

        private void ModelRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.IsChecked == true)
            {
                CurrentConfig.SelectedModel = rb.Content.ToString();
                System.Diagnostics.Debug.WriteLine($"Модель изменена: {CurrentConfig.SelectedModel}");

                // Присваиваем цену в зависимости от модели
                switch (CurrentConfig.SelectedModel)
                {
                    case "Toyota Camry":
                        CurrentConfig.ModelPrice = 2500000;
                        break;
                    case "Ford Mustang":
                        CurrentConfig.ModelPrice = 5650000;
                        break;
                    case "Lada Niva":
                        CurrentConfig.ModelPrice = 1500000;
                        break;
                }
            }

           
        }

        private void EngineRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.IsChecked == true)
            {
                CurrentConfig.SelectedEngine = rb.Content.ToString();

                // Присваиваем цену в зависимости от двигателя
                switch (CurrentConfig.SelectedEngine)
                {
                    case "ДВС":
                        CurrentConfig.EnginePrice = 250000;
                        break;
                    case "Электрический":
                        CurrentConfig.EnginePrice = 500000;
                        break;
                    case "Гибрид":
                        CurrentConfig.EnginePrice = 100000;
                        break;
                }
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Передача CurrentConfig: {CurrentConfig}");
            var page2 = new Page2(CurrentConfig); // передаём объект
            this.NavigationService.Navigate(page2);

        }

    }
}
