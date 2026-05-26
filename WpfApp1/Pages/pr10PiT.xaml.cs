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
using System.Windows.Shapes;
using System.Diagnostics;

namespace WpfApp1.Pages
{
    /// <summary>
    /// Логика взаимодействия для pr10PiT.xaml
    /// </summary>
    public partial class pr10PiT : Window
    {
        public pr10PiT()
        {
            InitializeComponent();
        }

        private void SpravkaButton_Click(object sender, RoutedEventArgs e)
        {      
            Process.Start("pages.chm");
        }
    }
}
