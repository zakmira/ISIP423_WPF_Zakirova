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
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            MainFrame.Navigate(new BuildPage { DataContext = _viewModel });
        }

        private void Build_Click(object sender, RoutedEventArgs e)
        {
            _viewModel = new MainViewModel();
            MainFrame.Navigate(new BuildPage { DataContext = _viewModel });
        }

        private void SavedBuilds_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SavedBuildsPage());
        }
    }
}
