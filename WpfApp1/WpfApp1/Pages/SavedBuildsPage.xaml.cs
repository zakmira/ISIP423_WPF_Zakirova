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
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    /// <summary>
    /// Логика взаимодействия для SavedBuildsPage.xaml
    /// </summary>
    public partial class SavedBuildsPage : Page
    {
        private MainViewModel _viewModel;

        public SavedBuildsPage()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is Assembly assembly)
            {
                // Загружаем выбранную сборку в BuildPage
                var buildPage = new BuildPage();
                var vm = new MainViewModel();
                
                // Копируем детали сборки
                foreach (var part in assembly.Parts)
                {
                    vm.SelectedParts.Add(part);
                }
                
                vm.AssemblyName = assembly.Name;
                vm.AssemblyAuthor = assembly.Author;
                
                buildPage.DataContext = vm;
                NavigationService?.Navigate(buildPage);
            }
        }
    }
}
