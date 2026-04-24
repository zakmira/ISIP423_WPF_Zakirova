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

namespace WpfApp17.Pages.Manager
{
    /// <summary>
    /// Логика взаимодействия для ManufacturerEdit.xaml
    /// </summary>
    public partial class ManufacturerEdit : Window
    {
        private Manufacturers _manufacturer;

        public ManufacturerEdit(Manufacturers manufacturer)
        {
            InitializeComponent();
            _manufacturer = manufacturer;

            if (_manufacturer != null)
            {
                Title = "Редактирование производителя";
                Name.Text = _manufacturer.Name;
                Country.Text = _manufacturer.Country;
            }
            else
            {
                Title = "Новый производитель";
                _manufacturer = new Manufacturers();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                MessageBox.Show("Введите название производителя");
                Name.Focus();
                return;
            }

            _manufacturer.Name = Name.Text.Trim();
            _manufacturer.Country = Country.Text?.Trim();

            if (_manufacturer.Id == 0)
            {
                Core.Context.Manufacturers.Add(_manufacturer);
            }

            Core.Context.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}