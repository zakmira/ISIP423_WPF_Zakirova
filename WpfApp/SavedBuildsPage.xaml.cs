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

namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для SavedBuildsPage.xaml
    /// </summary>
    public partial class SavedBuildsPage : Page
    {
        private assembly_ selectedAssembly;

        public SavedBuildsPage()
        {
            InitializeComponent();
            LoadBuilds();
        }

        private void LoadBuilds()
        {
            var builds = Core.Context.assembly_.ToList();

            var buildsWithCount = builds.Select(a => new
            {
                a.id,
                a.name,
                a.author,
                PartsCount = Core.Context.partassembly_.Count(p => p.assemblyid == a.id)
            }).ToList();

            BuildsList.ItemsSource = buildsWithCount;
        }

        private void BuildsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BuildsList.SelectedItem != null)
            {
                var selected = BuildsList.SelectedItem;
                int id = (int)selected.GetType().GetProperty("id").GetValue(selected);
                selectedAssembly = Core.Context.assembly_.FirstOrDefault(a => a.id == id);
                LoadButton.IsEnabled = true;
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAssembly == null) return;

            var buildPage = new BuildPage();
            buildPage.LoadAssembly(selectedAssembly);

            NavigationService.Navigate(buildPage);
        }
    }
}
