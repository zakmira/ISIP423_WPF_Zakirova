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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для BuildPage.xaml
    /// </summary>
    public partial class BuildPage : Page
    {
        public BuildPage()
        {
            InitializeComponent();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is BasePart part)
            {
                var vm = DataContext as MainViewModel;
                vm?.SelectPartCommand.Execute(part);
            }
        }
    }
}
