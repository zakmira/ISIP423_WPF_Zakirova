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
using WpfApp1.Pages;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigated += MainFrame_Navigated; // ← подписка
            MainFrame.Navigate(new Pages.Page1());
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateProgressBar();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is Pages.Page5 currentPage5)
            {
                // Если есть несохранённые данные и форма не валидна
                if (currentPage5.HasUnsavedChanges && !currentPage5.IsFormValidNow)
                {
                    var result = MessageBox.Show(
                        "У вас есть несохранённые данные в заявке. Уйти без отправки?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        return; // Отменяем переход
                    }
                }
            }

            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            BackButton.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

            if (e.Content is Page page)
            {
                TitleTextBlock.Text = page.Title;
            }
        }

        private void UpdateProgressBar()
        {
            int step = MainFrame.Content switch
            {
                Pages.Page1 => 1,
                Pages.Page2 => 2,
                Pages.Page3 => 3,
                Pages.Page4 => 4,
                Pages.Page5 => 5,
                _ => 1
            };

            StepProgressBar.Value = step;
        }
    }
}
